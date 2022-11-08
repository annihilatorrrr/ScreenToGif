using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.ViewModel;

namespace ScreenToGif.Controls;

public class BaseRecorder : ExWindow
{
    public readonly ScreenRecorderViewModel ViewModel;

    public RecordingProject Project => ViewModel?.Project;

    public BaseRecorder()
    {
        DataContext = ViewModel = new ScreenRecorderViewModel();
    }
}