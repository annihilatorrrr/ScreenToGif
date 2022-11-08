using System.Windows;

namespace ScreenToGif.Domain.Events;

public class RegionSelectionChangedEventArgs : RoutedEventArgs
{
    public Rect NewSelection { get; set; }

    public double Scale { get; set; }

    public RegionSelectionChangedEventArgs(RoutedEvent routedEvent, Rect selection, double scale) : base(routedEvent)
    {
        NewSelection = selection;
        Scale = scale;
    }
}