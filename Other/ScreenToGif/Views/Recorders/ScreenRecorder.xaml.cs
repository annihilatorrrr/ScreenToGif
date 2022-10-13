using ScreenToGif.Controls;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Util.Settings;
using System.Windows;
using System.Windows.Input;

namespace ScreenToGif.Views.Recorders;

public partial class ScreenRecorder : RecorderWindow
{
    public ScreenRecorder()
    {
        InitializeComponent();

        RegisterCommands();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Selection == Rect.Empty || UserSettings.All.SelectionBehavior == RecorderSelectionBehaviors.AlwaysAsk)
        {
            var (monitor, selection) = await SelectRegion();

            ViewModel.Selection = selection;
            ViewModel.Monitor = monitor;
        }

        //Show selection and in the UI.
    }

    private void RegisterCommands()
    {
        CommandBindings.Clear();
        CommandBindings.AddRange(new CommandBindingCollection
        {
            //new CommandBinding(_viewModel.CloseCommand, (_, _) => Close(),
            //    (_, args) => args.CanExecute = Stage == RecorderStages.Stopped || (UserSettings.All.CaptureFrequency is CaptureFrequencies.Manual or CaptureFrequencies.Interaction && (Project == null || !Project.Any))),

            new CommandBinding(ViewModel.OptionsCommand, ShowOptions, (_, args) => args.CanExecute = ViewModel.CanOpenOptions),
            new CommandBinding(ViewModel.SwitchFrequencyCommand, SwitchFrequency, (_, args) => args.CanExecute = (args.Parameter != null && !args.Parameter.Equals("Switch")) || ViewModel.CanSwitchFrequency),

            new CommandBinding(ViewModel.RecordCommand, Record, (_, args) => args.CanExecute = ViewModel.CanRecord),
            new CommandBinding(ViewModel.PauseCommand, Pause, (_, args) => args.CanExecute = ViewModel.CanPause),
            new CommandBinding(ViewModel.SnapCommand, Snap, (_, args) => args.CanExecute = ViewModel.CanSnap),

            new CommandBinding(ViewModel.StopLargeCommand, Stop, (_, args) => args.CanExecute = ViewModel.CanStopLarge),
            new CommandBinding(ViewModel.StopCommand, Stop, (_, args) => args.CanExecute = ViewModel.CanStop),

            new CommandBinding(ViewModel.DiscardCommand, Discard, (_, args) => args.CanExecute = ViewModel.CanDiscard)
        });

        ViewModel.RefreshKeyGestures();
    }



    private void Discard(object sender, ExecutedRoutedEventArgs e)
    {
        
    }

    private void Stop(object sender, ExecutedRoutedEventArgs e)
    {
        
    }

    private void Snap(object sender, ExecutedRoutedEventArgs e)
    {
        
    }

    private void Pause(object sender, ExecutedRoutedEventArgs e)
    {
        
    }

    private async void Record(object sender, ExecutedRoutedEventArgs e)
    {
        await SelectRegion();
    }

    private void ShowOptions(object sender, ExecutedRoutedEventArgs e)
    {
        
    }
    
    private void SwitchFrequency(object sender, ExecutedRoutedEventArgs e)
    {

    }


    //Commands:
    //Pick region
    //Record/Snap
    //Pause
    //Stop
    //Discard
    //Options
}