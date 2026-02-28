using ImageMagick;
using MyCourse.Models.Exceptions.Infrastructure;

namespace MyCourse.Models.Services.Infrastructure;

public class MagickNetImagePersister : IImagePersister
{
    private readonly IWebHostEnvironment env;

    private readonly SemaphoreSlim semaphore;

    public MagickNetImagePersister(IWebHostEnvironment env)
    {
        ResourceLimits.Height = 4000;
        ResourceLimits.Width = 4000;
        semaphore = new SemaphoreSlim(2);
        this.env = env;
    }

    public async Task<string> SaveCourseImageAsync(int courseId, IFormFile formFile)
    {
        //The WaitAsync method also has an overload that accepts a timeout
        //For example, to wait at most 1 second:
        //await semaphore.WaitAsync(TimeSpan.FromSeconds(1));
        //If the timeout expires, the SemaphoreSlim will throw an exception (so at least it won't wait forever)
        await semaphore.WaitAsync();
        try
        {
            //Save the file
            string path = $"/Courses/{courseId}.jpg";
            string physicalPath = Path.Combine(env.WebRootPath, "Courses", $"{courseId}.jpg");

            using Stream inputStream = formFile.OpenReadStream();
            using MagickImage image = new(inputStream);

            //Manipulate the image
            int width = 300;  //TODO: retrieve these values from configuration
            int height = 300;
            MagickGeometry resizeGeometry = new(width, height)
            {
                FillArea = true
            };
            image.Resize(resizeGeometry);
            image.Crop(width, width, Gravity.Northwest);

            image.Quality = 70;
            image.Write(physicalPath, MagickFormat.Jpg);

            //Return the path to the saved file
            return path;
        }
        catch (Exception exc)
        {
            throw new ImagePersistenceException(exc);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
