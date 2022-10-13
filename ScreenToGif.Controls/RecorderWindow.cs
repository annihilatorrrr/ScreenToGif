using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel;
using System.Windows;

namespace ScreenToGif.Controls;

public class RecorderWindow : ExWindow
{
    protected internal readonly ScreenRecorderViewModel ViewModel;

    public RecordingProject Project => ViewModel?.Project;

    public RecorderWindow()
    {
        DataContext = ViewModel = new ScreenRecorderViewModel();

        LoadOptions();
    }

    private void LoadOptions()
    {
        ViewModel.CaptureFrequency = UserSettings.All.CaptureFrequency;

        switch (UserSettings.All.SelectionBehavior)
        {
            case RecorderSelectionBehaviors.AlwaysAsk:
                ViewModel.Selection = Rect.Empty;
                break;

            case RecorderSelectionBehaviors.RememberSize:
                ViewModel.Selection = UserSettings.All.SelectedRegion with { X = double.NaN, Y = double.NaN };
                break;

            default:
                ViewModel.Selection = UserSettings.All.SelectedRegion;
                break;
        }
    }

    private void PersistOptions()
    {
        UserSettings.All.CaptureFrequency = ViewModel.CaptureFrequency;
    }
}