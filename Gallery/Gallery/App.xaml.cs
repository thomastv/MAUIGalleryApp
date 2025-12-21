using Gallery.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gallery;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        // Initialize database when app starts
        Task.Run(async () =>
        {
            try
            {
                var databaseService = Handler?.MauiContext?.Services?.GetService<DatabaseService>();
                if (databaseService != null)
                {
                    await databaseService.InitializeDatabaseAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize database: {ex.Message}");
            }
        });
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}