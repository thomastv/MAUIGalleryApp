using CommunityToolkit.Mvvm.Input;
using Gallery.Models;

namespace Gallery.PageModels;

public interface IProjectTaskPageModel
{
	IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
	bool IsBusy { get; }
}