using Gallery.Data;
using Microsoft.EntityFrameworkCore;
using ImageModel = Gallery.Models.Image;
using TagModel = Gallery.Models.Tag;
using ImageTagModel = Gallery.Models.ImageTag;

namespace Gallery.Services;

public class ImageService
{
    private readonly GalleryContext _context;

    public ImageService(GalleryContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ImageModel>> GetAllImagesAsync()
    {
        return await _context.Images
            .Include(i => i.ImageTags)
            .ThenInclude(it => it.Tag)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<ImageModel?> GetImageByIdAsync(int id)
    {
        return await _context.Images
            .Include(i => i.ImageTags)
            .ThenInclude(it => it.Tag)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<ImageModel>> SearchImagesByTagsAsync(IEnumerable<string> tagNames)
    {
        if (!tagNames.Any())
        {
            return await GetAllImagesAsync();
        }

        var tagNamesList = tagNames.ToList();
        
        return await _context.Images
            .Include(i => i.ImageTags)
            .ThenInclude(it => it.Tag)
            .Where(i => i.ImageTags.Any(it => tagNamesList.Contains(it.Tag.Name)))
            .Distinct()
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ImageModel>> SearchImagesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllImagesAsync();
        }

        var lowerSearchTerm = searchTerm.ToLowerInvariant();
        
        return await _context.Images
            .Include(i => i.ImageTags)
            .ThenInclude(it => it.Tag)
            .Where(i => i.Title.ToLower().Contains(lowerSearchTerm) ||
                       i.Description.ToLower().Contains(lowerSearchTerm) ||
                       i.FileName.ToLower().Contains(lowerSearchTerm) ||
                       i.ImageTags.Any(it => it.Tag.Name.ToLower().Contains(lowerSearchTerm)))
            .Distinct()
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<ImageModel> AddImageAsync(string filePath, string fileName, string? title = null, string? description = null)
    {
        var fileInfo = new FileInfo(filePath);
        
        var image = new ImageModel
        {
            FilePath = filePath,
            FileName = fileName,
            Title = title ?? Path.GetFileNameWithoutExtension(fileName),
            Description = description ?? string.Empty,
            FileSize = fileInfo.Exists ? fileInfo.Length : 0,
            TakenAt = fileInfo.Exists ? fileInfo.CreationTime : DateTime.Now
        };

        _context.Images.Add(image);
        await _context.SaveChangesAsync();
        
        return image;
    }

    public async Task<bool> AddTagToImageAsync(int imageId, int tagId)
    {
        // Check if the relationship already exists
        var existingImageTag = await _context.ImageTags
            .FirstOrDefaultAsync(it => it.ImageId == imageId && it.TagId == tagId);

        if (existingImageTag != null)
            return false; // Relationship already exists

        var imageTag = new ImageTagModel
        {
            ImageId = imageId,
            TagId = tagId
        };

        _context.ImageTags.Add(imageTag);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> RemoveTagFromImageAsync(int imageId, int tagId)
    {
        var imageTag = await _context.ImageTags
            .FirstOrDefaultAsync(it => it.ImageId == imageId && it.TagId == tagId);

        if (imageTag == null)
            return false;

        _context.ImageTags.Remove(imageTag);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteImageAsync(int imageId)
    {
        var image = await _context.Images.FindAsync(imageId);
        if (image == null)
            return false;

        // Delete the physical file
        if (File.Exists(image.FilePath))
        {
            try
            {
                File.Delete(image.FilePath);
            }
            catch
            {
                // Log the error but continue with database deletion
            }
        }

        _context.Images.Remove(image);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> UpdateImageAsync(int imageId, string? title = null, string? description = null)
    {
        var image = await _context.Images.FindAsync(imageId);
        if (image == null)
            return false;

        if (title != null)
            image.Title = title;
        
        if (description != null)
            image.Description = description;

        await _context.SaveChangesAsync();
        return true;
    }
}