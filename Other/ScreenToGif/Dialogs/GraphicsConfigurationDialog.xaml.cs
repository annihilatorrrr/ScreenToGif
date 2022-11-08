using Microsoft.Win32;
using ScreenToGif.Controls;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Util;
using ScreenToGif.Util.Native;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScreenToGif.Dialogs;

public partial class GraphicsConfigurationDialog : ExWindow
{
    #region Properties

    private double _minLeft = SystemParameters.VirtualScreenLeft;
    private double _minTop = SystemParameters.VirtualScreenTop;
    private double _maxRight = SystemParameters.VirtualScreenWidth;
    private double _maxBottom = SystemParameters.VirtualScreenHeight;

    public Exception Exception { get; set; }

    public IMonitor Monitor { get; set; }

    #endregion

    public GraphicsConfigurationDialog()
    {
        InitializeComponent();

        SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (Exception == null)
            DetailsButton.IsEnabled = false;
    }

    private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
    {
        DetectScreens();
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        try
        {
            Process.Start("ms-settings:display-advancedgraphics");
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Impossible to open Windows Settings");
        }
    }

    private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!IsLoaded)
            return;

        SetViewPort(_minLeft, _maxRight, _minTop, _maxBottom);
    }

    private void DismissButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void DetailsButton_Click(object sender, RoutedEventArgs e)
    {
        var errorViewer = new ExceptionDialog { Exception = Exception };
        errorViewer.ShowDialog();
    }

    private void FeedbackButton_Click(object sender, RoutedEventArgs e)
    {
        var feedback = new FeedbackDialog { Topmost = true };

        if (feedback.ShowDialog() != true)
            return;

        if (App.ViewModel != null)
            App.ViewModel.SendFeedbackCommand.Execute(null);
    }

    private void PrepareOk()
    {
        //No Graphics Settings page prior to Windows 10, build 17093.
        if (Environment.OSVersion.Version.Major < 10 || Environment.OSVersion.Version.Build < 17093)
        {
            ActionTextBlock.SetResourceReference(TextBlock.TextProperty, "S.Warning.Graphics.Action.Legacy");
            HyperlinkTextBlock.Visibility = Visibility.Collapsed;
        }

        DetectScreens();
        DismissButton.Focus();
    }

    private void DetectScreens()
    {
        var monitors = MonitorHelper.AllMonitorsGranular();
        _minLeft = monitors.Min(m => m.NativeBounds.Left);
        _minTop = monitors.Min(m => m.NativeBounds.Top);
        _maxRight = monitors.Max(m => m.NativeBounds.Right);
        _maxBottom = monitors.Max(m => m.NativeBounds.Bottom);

        MainCanvas.Children.Clear();

        foreach (var monitor in monitors)
        {
            var rect = new Rectangle
            {
                Width = monitor.NativeBounds.Width,
                Height = monitor.NativeBounds.Height,
                StrokeThickness = 6
            };
            rect.SetResourceReference(Shape.StrokeProperty, "Brush.Foreground");
            rect.SetResourceReference(Shape.FillProperty, monitor.AdapterName == Monitor.AdapterName ? "Brush.Foreground.Medium" : "Brush.Foreground.Weak");

            var textBlock = new TextBlock
            {
                Text = monitor.AdapterName,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 26,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(15)
            };
            textBlock.SetResourceReference(TextBlock.ForegroundProperty, "Brush.Foreground");

            var viewbox = new Viewbox
            {
                Child = textBlock,
                Width = monitor.NativeBounds.Width,
                Height = monitor.NativeBounds.Height,
            };

            MainCanvas.Children.Add(rect);
            MainCanvas.Children.Add(viewbox);

            Canvas.SetLeft(rect, monitor.NativeBounds.Left);
            Canvas.SetTop(rect, monitor.NativeBounds.Top);
            Canvas.SetLeft(viewbox, monitor.NativeBounds.Left);
            Canvas.SetTop(viewbox, monitor.NativeBounds.Top);
            Panel.SetZIndex(rect, 1);
            Panel.SetZIndex(viewbox, 2);
        }

        MainCanvas.Width = Math.Abs(_minLeft) + Math.Abs(_maxRight);
        MainCanvas.Height = Math.Abs(_minTop) + Math.Abs(_maxBottom);
        MainCanvas.Measure(new Size(MainCanvas.Width, MainCanvas.Height));
        MainCanvas.Arrange(new Rect(MainCanvas.DesiredSize));

        SetViewPort(_minLeft, _maxRight, _minTop, _maxBottom);
    }

    private void SetViewPort(double minX, double maxX, double minY, double maxY)
    {
        var width = maxX - minX;
        var height = maxY - minY;

        var group = new TransformGroup();
        group.Children.Add(new TranslateTransform(-minX, -minY));
        group.Children.Add(new ScaleTransform(MainCanvas.ActualWidth / width, MainCanvas.ActualHeight / height));

        MainCanvas.RenderTransform = group;
    }

    public static bool Show(Exception exception, IMonitor monitor)
    {
        var dialog = new GraphicsConfigurationDialog
        {
            Exception = exception,
            Monitor = monitor
        };

        dialog.PrepareOk();
        var result = dialog.ShowDialog();

        return result.HasValue && result.Value;
    }
}