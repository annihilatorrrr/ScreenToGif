using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Native;
using ScreenToGif.Util.Native;
using ScreenToGif.Windows.Other;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenToGif.Util;

internal static class RegionSelectHelper
{
    internal class Selection
    {
        public IMonitor Monitor { get; set; }
            
        public Rect Region { get; set; }

        public Selection(IMonitor monitor, Rect region)
        {
            Monitor = monitor;
            Region = region;
        }
    }

    #region Properties

    private static TaskCompletionSource<Selection> _taskCompletionSource;

    private static readonly List<RegionSelector> Selectors = new();

    internal static bool IsSelecting => Selectors.Any(a => a.IsVisible && a.IsActive);

    #endregion

    internal static Task<Selection> Select(RegionSelectionModes mode, Rect previousRegion, IMonitor currentMonitor, bool quickSelection = false)
    {
        _taskCompletionSource = new TaskCompletionSource<Selection>();

        Selectors.Clear();

        var monitors = MonitorHelper.AllMonitorsGranular();

        //If in quick screen selection mode and there's just one screen, select that one.
        if (quickSelection && mode == RegionSelectionModes.Fullscreen && monitors.Count == 1)
            return Task.FromResult(new Selection(monitors.FirstOrDefault(), monitors[0].Bounds));

        foreach (var monitor in monitors)
        {
            var selector = new RegionSelector();
            //selector.Select(monitor, mode, monitor.Handle == currentMonitor?.Handle ? previousRegion : Rect.Empty, RegionSelected, RegionChanged, RegionGotHover, RegionAborted);

            Selectors.Add(selector);
        }

        //Return only when the region gets selected.
        return _taskCompletionSource.Task;
    }

    internal static void Abort()
    {
        RegionAborted();
    }


    private static void RegionSelected(Monitor monitor, Rect region)
    {
        foreach (var selector in Selectors)
            selector.CancelSelection();

        _taskCompletionSource.SetResult(new Selection(monitor, region));
    }

    private static void RegionChanged(Monitor monitor)
    {
        //When one monitor gets the focus, the other ones should be cleaned.
        foreach (var selector in Selectors.Where(w => w.Monitor.Handle != monitor.Handle))
            selector.ClearSelection();
    }

    private static void RegionGotHover(Monitor monitor)
    {
        //When one monitor gets the focus, the other ones should be cleaned.
        foreach (var selector in Selectors.Where(w => w.Monitor.Handle != monitor.Handle))
            selector.ClearHoverEffects();
    }

    private static void RegionAborted()
    {
        foreach (var selector in Selectors)
            selector.CancelSelection();

        _taskCompletionSource.SetResult(new Selection(null, Rect.Empty));
    }
}