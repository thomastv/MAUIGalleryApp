using Gallery.Models;

namespace Gallery.Pages;

public partial class ProjectDetailPage : ContentPage
{
	public ProjectDetailPage(ProjectDetailPageModel model)
	{
		InitializeComponent();

		BindingContext = model;
	}
}
