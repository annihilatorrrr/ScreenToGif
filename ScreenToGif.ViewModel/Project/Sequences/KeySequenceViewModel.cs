using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Project.Cached.Sequences;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.ViewModel.Project.Sequences.SubSequences;
using System.Collections.ObjectModel;

namespace ScreenToGif.ViewModel.Project.Sequences;

public class KeySequenceViewModel : SequenceViewModel
{
    private ObservableCollection<KeySubSequenceViewModel> _keyEvents;

    /// <summary>
    /// Each key press with its timings.
    /// </summary>
    public ObservableCollection<KeySubSequenceViewModel> KeyEvents
    {
        get => _keyEvents;
        set => SetProperty(ref _keyEvents, value);
    }

    public static KeySequenceViewModel FromModel(KeySequence sequence, IPreviewerViewModel baseViewModel)
    {
        return new KeySequenceViewModel
        {
            Id = sequence.Id,
            StartTime = sequence.StartTime,
            EndTime = sequence.EndTime,
            Opacity = sequence.Opacity,
            Background = sequence.Background,
            Effects = new ObservableCollection<object>(sequence.Effects), //TODO
            StreamPosition = sequence.StreamPosition,
            CachePath = sequence.CachePath,
            PreviewerViewModel = baseViewModel,
            //Left = sequence.Left,
            //Top = sequence.Top,
            //Width = sequence.Width,
            //Height = sequence.Height,
            //Angle = sequence.Angle,
            KeyEvents = new ObservableCollection<KeySubSequenceViewModel>(sequence.KeyEvents.Select(KeySubSequenceViewModel.FromModel).ToList())
        };
    }

    public static KeySequenceViewModel FromModel(RecordingProject project, IPreviewerViewModel baseViewModel)
    {
        //Are keys from recording 1 to 1 to editor?

        return null;
    }

    public override void RenderAt(IntPtr current, int canvasWidth, int canvasHeight, long timestamp, double quality, string cachePath)
    {
        //Rendering should use global settings for the style, position, size, etc.
    }
}