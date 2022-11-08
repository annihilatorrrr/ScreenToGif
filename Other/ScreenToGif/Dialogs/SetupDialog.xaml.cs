using ScreenToGif.Controls;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel;

namespace ScreenToGif.Dialogs;

public partial class SetupDialog : ExWindow
{
    private readonly SetupDialogViewModel _viewModel;

    public SetupDialog()
    {
        InitializeComponent();

        DataContext = _viewModel = new SetupDialogViewModel();
    }

    public static bool Ask()
    {
        var dialog = new SetupDialog();
        return dialog.ShowDialog() ?? false;
    }

    private void SelectButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        UserSettings.All.StartupWindow = (StartupWindows)_viewModel.StartupWindow;
        UserSettings.All.WindowAfterRecording = (AfterRecordingWindows)_viewModel.AfterRecording;

        DialogResult = true;
    }

    private void AskLaterButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        DialogResult = false;
    }
}