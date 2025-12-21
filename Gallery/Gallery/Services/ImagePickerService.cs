using CommunityToolkit.Maui.Storage;
using Gallery.Services;

namespace Gallery.Services;

public class ImagePickerService
{
    private readonly ImageService _imageService;

    public ImagePickerService(ImageService imageService)
    {
        _imageService = imageService;
    }

    public async Task<bool> PickAndAddImageAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select an image",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null)
                return false;

            // Copy file to app's local folder
            var localFilePath = await CopyFileToLocalStorageAsync(result);
            
            if (localFilePath == null)
                return false;

            // Add to database
            await _imageService.AddImageAsync(localFilePath, result.FileName);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> PickMultipleAndAddImagesAsync()
    {
        try
        {
            var results = await FilePicker.Default.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "Select images",
                FileTypes = FilePickerFileType.Images
            });

            if (results == null || !results.Any())
                return false;

            var successCount = 0;
            foreach (var result in results)
            {
                try
                {
                    var localFilePath = await CopyFileToLocalStorageAsync(result);
                    if (localFilePath != null)
                    {
                        await _imageService.AddImageAsync(localFilePath, result.FileName);
                        successCount++;
                    }
                }
                catch
                {
                    // Continue with other files even if one fails
                    continue;
                }
            }

            return successCount > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<string?> CopyFileToLocalStorageAsync(FileResult fileResult)
    {
        try
        {
            // Create a unique filename to avoid conflicts
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var extension = Path.GetExtension(fileResult.FileName);
            var uniqueFileName = $"{timestamp}_{Guid.NewGuid():N}{extension}";
            
            // Get the app's local storage directory
            var appDataDir = FileSystem.AppDataDirectory;
            var imagesDir = Path.Combine(appDataDir, "Images");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(imagesDir))
                Directory.CreateDirectory(imagesDir);
            
            var localFilePath = Path.Combine(imagesDir, uniqueFileName);
            
            // Copy the file
            using var sourceStream = await fileResult.OpenReadAsync();
            using var destinationStream = File.Create(localFilePath);
            await sourceStream.CopyToAsync(destinationStream);
            
            return localFilePath;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static bool IsImageFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".tiff" or ".svg";
    }
}