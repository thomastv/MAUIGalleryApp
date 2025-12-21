using Microsoft.EntityFrameworkCore;
using ImageModel = Gallery.Models.Image;
using TagModel = Gallery.Models.Tag;
using ImageTagModel = Gallery.Models.ImageTag;

namespace Gallery.Data;

public class GalleryContext : DbContext
{
    public GalleryContext(DbContextOptions<GalleryContext> options) : base(options)
    {
    }

    public DbSet<ImageModel> Images { get; set; }
    public DbSet<TagModel> Tags { get; set; }
    public DbSet<ImageTagModel> ImageTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Image entity
        modelBuilder.Entity<ImageModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => e.FilePath).IsUnique();
        });

        // Configure Tag entity
        modelBuilder.Entity<TagModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure ImageTag junction table
        modelBuilder.Entity<ImageTagModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Configure relationships
            entity.HasOne(e => e.Image)
                  .WithMany(i => i.ImageTags)
                  .HasForeignKey(e => e.ImageId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.ImageTags)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Prevent duplicate tags on the same image
            entity.HasIndex(e => new { e.ImageId, e.TagId }).IsUnique();
        });
    }
}