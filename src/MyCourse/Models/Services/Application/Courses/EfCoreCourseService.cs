using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels.Courses;
using MyCourse.Controllers;
using Ganss.XSS;

namespace MyCourse.Models.Services.Application.Courses;

public class EfCoreCourseService : ICourseService
{
    private readonly ILogger<EfCoreCourseService> logger;
    private readonly MyCourseDbContext dbContext;
    private readonly LinkGenerator linkGenerator;
    private readonly ITransactionLogger transactionLogger;
    private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
    private readonly IImagePersister imagePersister;
    private readonly IPaymentGateway paymentGateway;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IEmailClient emailClient;

    public EfCoreCourseService(IHttpContextAccessor httpContextAccessor,
                               ILogger<EfCoreCourseService> logger,
                               IEmailClient emailClient,
                               IImagePersister imagePersister,
                               IPaymentGateway paymentGateway,
                               MyCourseDbContext dbContext,
                               LinkGenerator linkGenerator,
                               ITransactionLogger transactionLogger,
                               IOptionsMonitor<CoursesOptions> coursesOptions)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.imagePersister = imagePersister;
        this.paymentGateway = paymentGateway;
        this.coursesOptions = coursesOptions;
        this.logger = logger;
        this.dbContext = dbContext;
        this.linkGenerator = linkGenerator;
        this.transactionLogger = transactionLogger;
        this.emailClient = emailClient;
    }

    public async Task<CourseDetailViewModel> GetCourseAsync(int id)
    {
        IQueryable<CourseDetailViewModel> queryLinq = dbContext.Courses
            .AsNoTracking()
            .Include(course => course.Lessons)
            .Where(course => course.Id == id)
            .Select(course => CourseDetailViewModel.FromEntity(course)); //Using static methods like FromEntity may cause inefficient queries. Keep the mapping inline in the lambda or use a custom extension method

        CourseDetailViewModel viewModel = await queryLinq.FirstOrDefaultAsync();
        //.FirstOrDefaultAsync(); //Returns null if the list is empty and never throws an exception
        //.SingleOrDefaultAsync(); //Accepts an empty list (returns null) but throws if more than 1 element is returned
        //.FirstAsync(); //Returns the first element; throws if the list is empty
        //.SingleAsync(); //Returns the first element; throws if the list is empty or contains more than one element

        if (viewModel == null)
        {
            logger.LogWarning("Course {id} not found", id);
            throw new CourseNotFoundException(id);
        }

        return viewModel;
    }

    public async Task<List<CourseViewModel>> GetBestRatingCoursesAsync()
    {
        CourseListInputModel inputModel = new(
            search: "",
            page: 1,
            orderby: "Rating",
            ascending: false,
            limit: coursesOptions.CurrentValue.InHome,
            orderOptions: coursesOptions.CurrentValue.Order);

        ListViewModel<CourseViewModel> result = await GetCoursesAsync(inputModel);
        return result.Results;
    }

    public async Task<List<CourseViewModel>> GetMostRecentCoursesAsync()
    {
        CourseListInputModel inputModel = new(
            search: "",
            page: 1,
            orderby: "Id",
            ascending: false,
            limit: coursesOptions.CurrentValue.InHome,
            orderOptions: coursesOptions.CurrentValue.Order);

        ListViewModel<CourseViewModel> result = await GetCoursesAsync(inputModel);
        return result.Results;
    }

    public async Task<ListViewModel<CourseViewModel>> GetCoursesAsync(CourseListInputModel model)
    {
        IQueryable<Course> baseQuery = dbContext.Courses;

        baseQuery = (model.OrderBy, model.Ascending) switch
        {
            ("Title", true) => baseQuery.OrderBy(course => course.Title),
            ("Title", false) => baseQuery.OrderByDescending(course => course.Title),
            ("Rating", true) => baseQuery.OrderBy(course => course.Rating),
            ("Rating", false) => baseQuery.OrderByDescending(course => course.Rating),
            ("CurrentPrice", true) => baseQuery.OrderBy(course => course.CurrentPrice.Amount),
            ("CurrentPrice", false) => baseQuery.OrderByDescending(course => course.CurrentPrice.Amount),
            ("Id", true) => baseQuery.OrderBy(course => course.Id),
            ("Id", false) => baseQuery.OrderByDescending(course => course.Id),
            _ => baseQuery
        };

        IQueryable<Course> queryLinq = baseQuery
            .Where(course => course.Title.Contains(model.Search))
            .AsNoTracking();

        List<CourseViewModel> courses = await queryLinq
            .Skip(model.Offset)
            .Take(model.Limit)
            .Select(course => CourseViewModel.FromEntity(course)) //When using static methods like FromEntity, the query may be inefficient. Keep the mapping inside the lambda or use a custom extension method
            .ToListAsync(); //The database query is sent here, when we materialise the results

        int totalCount = await queryLinq.CountAsync();

        ListViewModel<CourseViewModel> result = new()
        {
            Results = courses,
            TotalCount = totalCount
        };

        return result;
    }

    public async Task<CourseDetailViewModel> CreateCourseAsync(CourseCreateInputModel inputModel)
    {
        string title = inputModel.Title;
        string author;
        string authorId;

        try
        {
            author = httpContextAccessor.HttpContext.User.FindFirst("FullName").Value;
            authorId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        catch (NullReferenceException)
        {
            throw new UserUnknownException();
        }

        Course course = new(title, author, authorId);
        dbContext.Add(course);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exc) when ((exc.InnerException as SqliteException)?.SqliteErrorCode == 19)
        {
            throw new CourseTitleUnavailableException(title, exc);
        }

        return CourseDetailViewModel.FromEntity(course);
    }

    public async Task<CourseDetailViewModel> EditCourseAsync(CourseEditInputModel inputModel)
    {
        Course course = await dbContext.Courses.FindAsync(inputModel.Id);

        if (course == null)
        {
            throw new CourseNotFoundException(inputModel.Id);
        }

        course.ChangeTitle(inputModel.Title);
        course.ChangePrices(inputModel.FullPrice, inputModel.CurrentPrice);
        course.ChangeDescription(inputModel.Description);
        course.ChangeEmail(inputModel.Email);

        dbContext.Entry(course).Property(course => course.RowVersion).OriginalValue = inputModel.RowVersion;

        if (inputModel.Image != null)
        {
            try
            {
                string imagePath = await imagePersister.SaveCourseImageAsync(inputModel.Id, inputModel.Image);
                course.ChangeImagePath(imagePath);
            }
            catch (Exception exc)
            {
                throw new CourseImageInvalidException(inputModel.Id, exc);
            }
        }

        //dbContext.Update(course);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new OptimisticConcurrencyException();
        }
        catch (DbUpdateException exc) when ((exc.InnerException as SqliteException)?.SqliteErrorCode == 19)
        {
            throw new CourseTitleUnavailableException(inputModel.Title, exc);
        }

        return CourseDetailViewModel.FromEntity(course);
    }

    public async Task<bool> IsTitleAvailableAsync(string title, int id)
    {
        //await dbContext.Courses.AnyAsync(course => course.Title == title);
        bool titleExists = await dbContext.Courses.AnyAsync(course => EF.Functions.Like(course.Title, title) && course.Id != id);
        return !titleExists;
    }

    public async Task<CourseEditInputModel> GetCourseForEditingAsync(int id)
    {
        IQueryable<CourseEditInputModel> queryLinq = dbContext.Courses
            .AsNoTracking()
            .Where(course => course.Id == id)
            .Select(course => CourseEditInputModel.FromEntity(course)); //When using static methods like FromEntity, the query may be inefficient. Keep the mapping inside the lambda or use a custom extension method

        CourseEditInputModel viewModel = await queryLinq.FirstOrDefaultAsync();

        if (viewModel == null)
        {
            logger.LogWarning("Course {id} not found", id);
            throw new CourseNotFoundException(id);
        }

        return viewModel;
    }

    public async Task DeleteCourseAsync(CourseDeleteInputModel inputModel)
    {
        Course course = await dbContext.Courses.FindAsync(inputModel.Id);

        if (course == null)
        {
            throw new CourseNotFoundException(inputModel.Id);
        }

        course.ChangeStatus(CourseStatus.Deleted);
        await dbContext.SaveChangesAsync();
    }

    public async Task SendQuestionToCourseAuthorAsync(int courseId, string question)
    {
        // Sanitise user input
        question = new HtmlSanitizer(allowedTags: new string[0]).Sanitize(question);

        // Retrieve course information
        Course course = await dbContext.Courses.FindAsync(courseId);

        if (course == null)
        {
            logger.LogWarning("Course {id} not found", courseId);
            throw new CourseNotFoundException(courseId);
        }

        string courseTitle = course.Title;
        string courseEmail = course.Email;

        // Retrieve the information of the user sending the question
        string userFullName;
        string userEmail;

        try
        {
            userFullName = httpContextAccessor.HttpContext.User.FindFirst("FullName").Value;
            userEmail = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Email).Value;
        }
        catch (NullReferenceException)
        {
            throw new UserUnknownException();
        }

        // Sanitise the user's question
        question = new HtmlSanitizer(allowedTags: new string[0]).Sanitize(question);

        // Compose the email body
        string subject = $@"Question about your course "{courseTitle}"";
        string message = $@"<p>User {userFullName} (<a href=""{userEmail}"">{userEmail}</a>)
                                sent you the following question about your course "{courseTitle}".</p>
                                <p>{question}</p>";

        // Send the question
        try
        {
            await emailClient.SendEmailAsync(courseEmail, userEmail, subject, message);
        }
        catch
        {
            throw new SendException();
        }
    }

    public Task<string> GetCourseAuthorIdAsync(int courseId)
    {
        return dbContext.Courses
                        .Where(course => course.Id == courseId)
                        .Select(course => course.AuthorId)
                        .FirstOrDefaultAsync();
    }

    public Task<int> GetCourseCountByAuthorIdAsync(string authorId)
    {
        return dbContext.Courses
                        .Where(course => course.AuthorId == authorId)
                        .CountAsync();
    }

    public async Task SubscribeCourseAsync(CourseSubscribeInputModel inputModel)
    {
        Subscription subscription = new(inputModel.UserId, inputModel.CourseId)
        {
            PaymentDate = inputModel.PaymentDate,
            PaymentType = inputModel.PaymentType,
            Paid = inputModel.Paid,
            TransactionId = inputModel.TransactionId
        };

        dbContext.Subscriptions.Add(subscription);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new CourseSubscriptionException(inputModel.CourseId);
        }
        catch (Exception)
        {
            await transactionLogger.LogTransactionAsync(inputModel);
        }
    }

    public Task<bool> IsCourseSubscribedAsync(int courseId, string userId)
    {
        return dbContext.Subscriptions.Where(subscription => subscription.CourseId == courseId && subscription.UserId == userId).AnyAsync();
    }

    public async Task<string> GetPaymentUrlAsync(int courseId)
    {
        CourseDetailViewModel viewModel = await GetCourseAsync(courseId);

        CoursePayInputModel inputModel = new()
        {
            CourseId = courseId,
            UserId = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
            Description = viewModel.Title,
            Price = viewModel.CurrentPrice,
            ReturnUrl = linkGenerator.GetUriByAction(httpContextAccessor.HttpContext,
                      action: nameof(CoursesController.Subscribe),
                      controller: "Courses",
                      values: new { id = courseId }),
            CancelUrl = linkGenerator.GetUriByAction(httpContextAccessor.HttpContext,
                      action: nameof(CoursesController.Detail),
                      controller: "Courses",
                      values: new { id = courseId })
        };

        return await paymentGateway.GetPaymentUrlAsync(inputModel);
    }

    public Task<CourseSubscribeInputModel> CapturePaymentAsync(int courseId, string token)
    {
        return paymentGateway.CapturePaymentAsync(token);
    }

    public async Task<int?> GetCourseVoteAsync(int courseId)
    {
        string userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        Subscription subscription = await dbContext.Subscriptions.SingleOrDefaultAsync(subscription => subscription.CourseId == courseId && subscription.UserId == userId);
        if (subscription == null)
        {
            throw new CourseSubscriptionNotFoundException(courseId);
        }

        return subscription.Vote;
    }

    public async Task VoteCourseAsync(CourseVoteInputModel inputModel)
    {
        if (inputModel.Vote < 1 || inputModel.Vote > 5)
        {
            throw new InvalidVoteException(inputModel.Vote);
        }

        string userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        Subscription subscription = await dbContext.Subscriptions.SingleOrDefaultAsync(subscription => subscription.CourseId == inputModel.Id && subscription.UserId == userId);
        if (subscription == null)
        {
            throw new CourseSubscriptionNotFoundException(inputModel.Id);
        }

        subscription.Vote = inputModel.Vote;
        await dbContext.SaveChangesAsync();
    }
}
