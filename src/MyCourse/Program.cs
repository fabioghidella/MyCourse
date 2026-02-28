namespace MyCourse;

public class Program
{
    public static void Main(string[] args)
    {
        // Various examples for using the new builder: https://docs.microsoft.com/en-us/aspnet/core/migration/50-to-60-samples
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        Startup startup = new(builder.Configuration);

        // Register services for dependency injection (ConfigureServices method)
        startup.ConfigureServices(builder.Services);

        WebApplication app = builder.Build();

        // Set up the middleware pipeline (Configure method)
        startup.Configure(app);

        app.Run();
    }
}