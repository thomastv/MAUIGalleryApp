using Gallery.Models;
using Gallery.PageModels;

namespace Gallery.Pages;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}