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

    //Props:
    //Show on system tray.
    //First window: Nothing (needs system tray), Recorders, Editor, Startup/Welcome
    //After recorder: Save, Editor

    //Recorders:
    //Opens: Recorders
    //After recording, open: Save Dialog with simple editor, Advanced Editor

    //Startup:
    //Opens: Startup Window
    //Then: Open any window
    //After recording, open: Save Dialog with simple editor, Advanced Editor

    //Editor:
    //Opens: Editor

    //Nothing:
    //Opens: Nothing 
    //Then: Open any window via command or via system tray icon
    //After recording, open: Save Dialog with simple editor, Advanced Editor

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