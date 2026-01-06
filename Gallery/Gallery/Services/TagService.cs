using Gallery.Data;
using Microsoft.EntityFrameworkCore;
using TagModel = Gallery.Models.Tag;

namespace Gallery.Services;

public class TagService(GalleryContext context)
{
    public async Task<IEnumerable<TagModel>> GetAllTagsAsync()
    {
        return await context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<TagModel?> GetTagByIdAsync(int id)
    {
        return await context.Tags.FindAsync(id);
    }

    public async Task<TagModel?> GetTagByNameAsync(string name)
    {
        return await context.Tags
            .FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<IEnumerable<TagModel>> SearchTagsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllTagsAsync();

        return await context.Tags
            .Where(t => t.Name.Contains(searchTerm) || t.Description.Contains(searchTerm))
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<TagModel> CreateTagAsync(string name, string? description = null)
    {
        // Check if tag already exists
        var existingTag = await GetTagByNameAsync(name);
        if (existingTag != null)
            return existingTag;

        var tag = new TagModel
        {
            Name = name,
            Description = description ?? string.Empty
        };

        context.Tags.Add(tag);
        await context.SaveChangesAsync();
        
        return tag;
    }

    public async Task<bool> UpdateTagAsync(int tagId, string? name = null, string? description = null)
    {
        var tag = await context.Tags.FindAsync(tagId);
        if (tag == null)
            return false;

        if (!string.IsNullOrWhiteSpace(name))
        {
            // Check if another tag with this name exists
            var existingTag = await GetTagByNameAsync(name);
            if (existingTag != null && existingTag.Id != tagId)
                return false; // Name conflict
            
            tag.Name = name;
        }

        if (description != null)
            tag.Description = description;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTagAsync(int tagId)
    {
        var tag = await context.Tags
            .Include(t => t.ImageTags)
            .FirstOrDefaultAsync(t => t.Id == tagId);
        
        if (tag == null)
            return false;

        // Remove all image-tag relationships
        context.ImageTags.RemoveRange(tag.ImageTags);
        context.Tags.Remove(tag);
        
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TagModel>> GetTagsForImageAsync(int imageId)
    {
        return await context.ImageTags
            .Where(it => it.ImageId == imageId)
            .Select(it => it.Tag)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<int> GetImageCountForTagAsync(int tagId)
    {
        return await context.ImageTags
            .CountAsync(it => it.TagId == tagId);
    }
}