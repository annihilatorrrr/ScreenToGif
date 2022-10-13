using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Util.Native;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenToGif.Views.Recorders;

public partial class ScreenRecorder
{
    private static TaskCompletionSource<(IMonitor, Rect)> _taskCompletionSource;

    private static readonly List<RegionSelector> Selectors = new();

    internal static bool IsSelecting => Selectors.Any(a => a.IsVisible && a.IsActive);

    private void RegionSelected(IMonitor monitor, Rect region)
    {
        foreach (var selector in Selectors)
            selector.Close();

        WindowState = WindowState.Normal;

        _taskCompletionSource.SetResult((monitor, region));
    }

    private void RegionGotHover(IMonitor monitor)
    {
        //When one monitor gets the focus, the other ones should be cleaned.
        foreach (var selector in Selectors)//.Where(w => w.Monitor.Handle != monitor.Handle))
            selector.ClearHoverEffects();
    }

    private void ModeChanged(RegionSelectionModes mode)
    {
        //When one monitor gets the focus, the other ones should be cleaned.
        foreach (var selector in Selectors) //.Where(w => w.Monitor.Handle != monitor.Handle))
            selector.SelectElement.Mode = mode;
    }

    private void RegionAborted()
    {
        foreach (var selector in Selectors)
            selector.Close();

        WindowState = WindowState.Normal;

        _taskCompletionSource.SetResult((null, Rect.Empty));
    }

    private Task<(IMonitor, Rect)> SelectRegion()
    {
        Selectors.Clear();

        WindowState = WindowState.Minimized;

        var monitors = MonitorHelper.AllMonitorsGranular();

        foreach (var monitor in monitors)
        {
            var selector = new RegionSelector();
            selector.Select(monitor, RegionSelected, RegionGotHover, ModeChanged, RegionAborted);

            Selectors.Add(selector);
        }

        //Return only when the region gets selected.
        _taskCompletionSource = new TaskCompletionSource<(IMonitor, Rect)>();

        return _taskCompletionSource.Task;
    }
}