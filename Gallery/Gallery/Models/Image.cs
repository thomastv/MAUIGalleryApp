using System.ComponentModel.DataAnnotations;

namespace Gallery.Models;

public class Image
{
    public int Id { get; set; }
    
    [Required]
    public string FilePath { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime TakenAt { get; set; }
    
    // Navigation property
    public ICollection<ImageTag> ImageTags { get; set; } = new List<ImageTag>();
}