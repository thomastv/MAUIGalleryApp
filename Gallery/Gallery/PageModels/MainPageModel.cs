using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gallery.Services;
using System.Collections.ObjectModel;
using ImageModel = Gallery.Models.Image;
using TagModel = Gallery.Models.Tag;

namespace Gallery.PageModels;

public partial class MainPageModel : ObservableObject
{
    private readonly ImageService _imageService;
    private readonly TagService _tagService;
    private readonly ImagePickerService _imagePickerService;

    public MainPageModel(ImageService imageService, TagService tagService, ImagePickerService imagePickerService)
    {
        _imageService = imageService;
        _tagService = tagService;
        _imagePickerService = imagePickerService;
    }

    [ObservableProperty]
    private ObservableCollection<ImageModel> images = new();

    [ObservableProperty]
    private ObservableCollection<TagModel> filterTags = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private ImageModel? selectedImage;

    [RelayCommand]
    private async Task LoadImages()
    {
        try
        {
            IsLoading = true;
            var imageList = await _imageService.GetAllImagesAsync();
            Images.Clear();
            
            foreach (var image in imageList)
            {
                Images.Add(image);
            }
        }
        catch (Exception ex)
        {
            // Handle error - could show a user message
            System.Diagnostics.Debug.WriteLine($"Error loading images: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Search()
    {
        try
        {
            IsLoading = true;

            IEnumerable<ImageModel> results;
            
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                // Use comprehensive search that includes title, description, filename, and tags
                results = await _imageService.SearchImagesAsync(SearchText);
            }
            else if (FilterTags.Any())
            {
                // Search by filter tags
                var tagNames = FilterTags.Select(t => t.Name);
                results = await _imageService.SearchImagesByTagsAsync(tagNames);
            }
            else
            {
                // No search criteria, load all images
                results = await _imageService.GetAllImagesAsync();
            }

            Images.Clear();
            foreach (var image in results)
            {
                Images.Add(image);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching images: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadImages();
    }

    [RelayCommand]
    private async Task AddImage()
    {
        try
        {
            IsLoading = true;
            var success = await _imagePickerService.PickAndAddImageAsync();
            
            if (success)
            {
                await LoadImages(); // Refresh the list
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding image: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddMultipleImages()
    {
        try
        {
            IsLoading = true;
            var success = await _imagePickerService.PickMultipleAndAddImagesAsync();
            
            if (success)
            {
                await LoadImages(); // Refresh the list
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding multiple images: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ImageSelected()
    {
        if (SelectedImage == null)
            return;

        // Navigate to image detail page
        var navigationParameter = new Dictionary<string, object>
        {
            { "Image", SelectedImage }
        };
        
        await Shell.Current.GoToAsync("//ImageDetail", navigationParameter);
        
        // Clear selection
        SelectedImage = null;
    }

    [RelayCommand]
    private void RemoveFilterTag(Gallery.Models.Tag tag)
    {
        var tagToRemove = FilterTags.FirstOrDefault(t => t.Id == tag.Id);
        if (tagToRemove != null)
        {
            FilterTags.Remove(tagToRemove);
            // Trigger search with updated filter
            _ = Search();
        }
    }

    public async Task AddFilterTag(Gallery.Models.Tag tag)
    {
        if (!FilterTags.Any(t => t.Id == tag.Id))
        {
            var tagModel = new TagModel
            {
                Id = tag.Id,
                Name = tag.Name,
                Description = tag.Description,
                CreatedAt = tag.CreatedAt
            };
            FilterTags.Add(tagModel);
            await Search(); // Trigger search with new filter
        }
    }
}