using System.ComponentModel.DataAnnotations;

namespace Gallery.Models;

public class ImageTag
{
    public int Id { get; set; }
    
    [Required]
    public int ImageId { get; set; }
    public Image Image { get; set; } = null!;
    
    [Required]
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}