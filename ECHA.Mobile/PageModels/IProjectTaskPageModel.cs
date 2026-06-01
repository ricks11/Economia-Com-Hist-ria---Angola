using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Models;

namespace ECHA.Mobile.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}