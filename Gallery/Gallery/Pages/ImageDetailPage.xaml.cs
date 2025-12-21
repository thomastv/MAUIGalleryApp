using Gallery.PageModels;
using ImageModel = Gallery.Models.Image;

namespace Gallery.Pages;

[QueryProperty(nameof(Image), "Image")]
public partial class ImageDetailPage : ContentPage
{
    public ImageDetailPage(ImageDetailPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public ImageModel? Image
    {
        get => (BindingContext as ImageDetailPageModel)?.Image;
        set
        {
            if (BindingContext is ImageDetailPageModel viewModel && value != null)
            {
                viewModel.Image = value;
            }
        }
    }
}