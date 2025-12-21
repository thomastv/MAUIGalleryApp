using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gallery.Services;
using System.Collections.ObjectModel;
using ImageModel = Gallery.Models.Image;
using TagModel = Gallery.Models.Tag;

namespace Gallery.PageModels;

public partial class ImageDetailPageModel : ObservableObject
{
    private readonly ImageService _imageService;
    private readonly TagService _tagService;

    public ImageDetailPageModel(ImageService imageService, TagService tagService)
    {
        _imageService = imageService;
        _tagService = tagService;
    }

    [ObservableProperty]
    private ImageModel? image;

    [ObservableProperty]
    private ObservableCollection<TagModel> imageTags = new();

    [ObservableProperty]
    private ObservableCollection<TagModel> suggestedTags = new();

    [ObservableProperty]
    private string newTagName = string.Empty;

    [ObservableProperty]
    private string imageTitle = string.Empty;

    [ObservableProperty]
    private string imageDescription = string.Empty;

    [ObservableProperty]
    private bool isEditMode = false;

    [ObservableProperty]
    private bool isLoading = false;

    public string FileSizeFormatted
    {
        get
        {
            if (Image == null) return string.Empty;
            
            var size = Image.FileSize;
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = size;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
    }

    [RelayCommand]
    private async Task Load()
    {
        if (Image == null) return;

        try
        {
            IsLoading = true;

            // Load fresh image data with tags
            var freshImage = await _imageService.GetImageByIdAsync(Image.Id);
            if (freshImage is not null)
            {
                Image = freshImage;
                ImageTitle = freshImage.Title;
                ImageDescription = freshImage.Description;
            }

            // Load current tags
            var currentTags = await _tagService.GetTagsForImageAsync(Image.Id);
            ImageTags.Clear();
            foreach (var tag in currentTags)
            {
                ImageTags.Add(tag);
            }

            // Load suggested tags (all tags not currently assigned)
            var allTags = await _tagService.GetAllTagsAsync();
            var currentTagIds = ImageTags.Select(t => t.Id).ToHashSet();
            
            SuggestedTags.Clear();
            foreach (var tag in allTags.Where(t => !currentTagIds.Contains(t.Id)))
            {
                SuggestedTags.Add(tag);
            }

            OnPropertyChanged(nameof(FileSizeFormatted));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading image details: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Edit()
    {
        IsEditMode = true;
    }

    [RelayCommand]
    private async Task SaveChanges()
    {
        if (Image == null) return;

        try
        {
            IsLoading = true;
            
            var success = await _imageService.UpdateImageAsync(Image.Id, ImageTitle, ImageDescription);
            
            if (success)
            {
                Image.Title = ImageTitle;
                Image.Description = ImageDescription;
                IsEditMode = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving changes: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        if (Image != null)
        {
            ImageTitle = Image.Title;
            ImageDescription = Image.Description;
        }
        IsEditMode = false;
    }

    [RelayCommand]
    private async Task AddTag()
    {
        if (string.IsNullOrWhiteSpace(NewTagName) || Image == null)
            return;

        try
        {
            IsLoading = true;

            // Create or get existing tag
            var tag = await _tagService.CreateTagAsync(NewTagName.Trim());

            // Add tag to image
            var success = await _imageService.AddTagToImageAsync(Image.Id, tag.Id);

            if (success)
            {
                // Update UI
                ImageTags.Add(tag);
                SuggestedTags.Remove(tag);
                NewTagName = string.Empty;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding tag: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddSuggestedTag(TagModel tag)
    {
        if (tag == null || Image == null) return;

        try
        {
            IsLoading = true;

            var success = await _imageService.AddTagToImageAsync(Image.Id, tag.Id);

            if (success)
            {
                ImageTags.Add(tag);
                SuggestedTags.Remove(tag);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding suggested tag: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RemoveTag(TagModel tag)
    {
        if (tag == null || Image == null) return;

        try
        {
            IsLoading = true;

            var success = await _imageService.RemoveTagFromImageAsync(Image.Id, tag.Id);

            if (success)
            {
                ImageTags.Remove(tag);
                SuggestedTags.Add(tag);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error removing tag: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (Image == null) return;

        bool answer = await Application.Current!.MainPage!.DisplayAlert(
            "Delete Image", 
            "Are you sure you want to delete this image? This action cannot be undone.", 
            "Yes", 
            "No");

        if (!answer) return;

        try
        {
            IsLoading = true;

            var success = await _imageService.DeleteImageAsync(Image.Id);

            if (success)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting image: {ex.Message}");
            await Application.Current!.MainPage!.DisplayAlert("Error", "Failed to delete image.", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnImageChanged(ImageModel? value)
    {
        if (value != null)
        {
            ImageTitle = value.Title;
            ImageDescription = value.Description;
            OnPropertyChanged(nameof(FileSizeFormatted));
        }
    }
}