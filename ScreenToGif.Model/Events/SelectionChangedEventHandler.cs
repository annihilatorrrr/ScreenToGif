namespace ScreenToGif.Domain.Events;

/// <summary>
/// The delegate to use for handlers that receive RegionSelectionChangedEventArgs.
/// </summary>
public delegate void RegionSelectionChangedEventHandler(object sender, RegionSelectionChangedEventArgs e);