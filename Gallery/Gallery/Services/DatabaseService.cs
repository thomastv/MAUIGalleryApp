using Gallery.Data;
using Microsoft.EntityFrameworkCore;
using TagModel = Gallery.Models.Tag;

namespace Gallery.Services;

public class DatabaseService
{
    private readonly GalleryContext _context;

    public DatabaseService(GalleryContext context)
    {
        _context = context;
    }

    public async Task InitializeDatabaseAsync()
    {
        try
        {
            // Ensure the database is created
            await _context.Database.EnsureCreatedAsync();
            
            // Seed default tags if none exist
            if (!await _context.Tags.AnyAsync())
            {
                await SeedDefaultTagsAsync();
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            throw new InvalidOperationException("Failed to initialize database.", ex);
        }
    }

    private async Task SeedDefaultTagsAsync()
    {
        var defaultTags = new[]
        {
            new TagModel { Name = "Family", Description = "Family photos and gatherings" },
            new TagModel { Name = "Travel", Description = "Travel and vacation photos" },
            new TagModel { Name = "Nature", Description = "Nature and outdoor photography" },
            new TagModel { Name = "Events", Description = "Special events and celebrations" },
            new TagModel { Name = "Portrait", Description = "Portrait photography" },
            new TagModel { Name = "Food", Description = "Food and culinary photos" },
            new TagModel { Name = "Architecture", Description = "Buildings and architectural photography" },
            new TagModel { Name = "Animals", Description = "Pets and wildlife photos" }
        };

        await _context.Tags.AddRangeAsync(defaultTags);
        await _context.SaveChangesAsync();
    }
}