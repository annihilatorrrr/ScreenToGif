using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ScreenToGif.Controls;

public class ExSlider : Slider
{
    private Thumb _thumb;
    private bool _isDownOnSlider = false;
    
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _thumb = GetTemplateChild("HorizontalThumb") as Thumb;

        if (_thumb != null)
        {
            _thumb.MouseEnter += Thumb_MouseEnter;
            _thumb.LostMouseCapture += Thumb_LostMouseCapture;
        }
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);

        _isDownOnSlider = true;
    }

    private void Thumb_MouseEnter(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || !_isDownOnSlider)
            return;

        // the left button is pressed on mouse enter 
        // so the thumb must have been moved under the mouse 
        // in response to a click on the track. 
        // Generate a MouseLeftButtonDown event. 
        var args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left) { RoutedEvent = MouseLeftButtonDownEvent };
        (sender as Thumb)?.RaiseEvent(args);
    }

    private void Thumb_LostMouseCapture(object sender, EventArgs e)
    {
        _isDownOnSlider = false;
    }
}