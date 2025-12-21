using CommunityToolkit.Maui;
using Gallery.Data;
using Gallery.Pages;
using Gallery.PageModels;
using Gallery.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;

namespace Gallery;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionToolkit()
            .ConfigureMauiHandlers(handlers =>
            {
#if WINDOWS
                Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
                {
                    handler.PlatformView.SingleSelectionFollowsFocus = false;
                });
#endif
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
            });

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddLogging(configure => configure.AddDebug());
#endif

        // Configure Entity Framework
        builder.Services.AddDbContext<GalleryContext>(options =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "gallery.db");
            options.UseSqlite($"Data Source={dbPath}");
        });

        // Register Services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddScoped<ImageService>();
        builder.Services.AddScoped<TagService>();
        builder.Services.AddScoped<ImagePickerService>();

        // Register Pages and ViewModels
        builder.Services.AddSingleton<MainPageModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ImageDetailPageModel>();
        builder.Services.AddTransient<ImageDetailPage>();

        return builder.Build();
    }
}
