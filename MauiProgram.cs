using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HyperDev;

public static class MauiProgram {
    // Expose the built service provider so XAML-created pages can resolve services at runtime
    public static IServiceProvider Services { get; private set; } = null!;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton(sp =>
        {
            var httpClient = new System.Net.Http.HttpClient();

            var configuration = sp.GetRequiredService<IConfiguration>();
            var token = configuration["GitHub:PersonalAccessToken"];
            if(string.IsNullOrEmpty(token))
                throw new InvalidOperationException("GitHub personal access token is not configured. Please set it in appsettings.json under the 'GitHub:PersonalAccessToken' key.");

            System.Diagnostics.Debug.WriteLine($"GitHub token loaded");
            return new src.Services.GitHubProjectService(httpClient, token);
        });

        var app = builder.Build();

        Services = app.Services;

        return app;
    }
}
