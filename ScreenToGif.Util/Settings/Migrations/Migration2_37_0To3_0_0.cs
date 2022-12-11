using ScreenToGif.Domain.Models;

namespace ScreenToGif.Settings.Migrations;

internal class Migration2_37_0To3_0_0
{
    internal static bool Up(List<Property> properties)
    {
        var startup = properties.FirstOrDefault(a => a.Key == "StartUp");

        if (startup?.Value == "5")
        {
            startup.Value = "0";

            var startMinimized = properties.FirstOrDefault(a => a.Key == "StartMinimized");
            startMinimized.Value = "true";

            var showNotificationIcon = properties.FirstOrDefault(a => a.Key == "ShowNotificationIcon");
            showNotificationIcon.Value = "true";
        }

        /// 0 - Startup window.
        /// 1 - Recorder window.
        /// 2 - Webcam window.
        /// 3 - Board window.
        /// 4 - Editor window.
        //Startup -> FirstWindow, int to StartupWindows
        /// 0: Screen recorder
        /// 1: Webcam recorder
        /// 2: Sketchboard recorder
        /// 3: Editor
        /// 4: Welcome
        /// 5: Nothing

        //LeftClickAction int to NotificationIconActions, same indexes
        //DoubleLeftClickAction int to NotificationIconActions, same indexes
        //MiddleClickAction int to NotificationIconActions, same indexes

        /// 0: None.
        /// 1: Startup
        /// 2: Screen recorder
        /// 3: Webcam recorder
        /// 4: Board recorder
        /// 5: Editor
        //LeftOpenWindowOriginal, int to StartupWindows
        //DoubleLeftOpenWindow, int to StartupWindows
        //MiddleOpenWindow, int to StartupWindows
        // 0: ScreenRecorder,
        // 1: WebcamRecorder,
        // 2: SketchboardRecorder,
        // 3: Editor,
        // 4: Welcome,
        // 5: None,

        var exportPresets = properties.FirstOrDefault(a => a.Key == "ExportPresets");

        if (exportPresets != null)
        {
            foreach (var presetsChild in exportPresets.Children)
            {
                //clr-namespace:ScreenToGif.ViewModel.ExportPresets.AnimatedImage.Gif;assembly=ScreenToGif.ViewModel
                //clr-namespace:ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;assembly=ScreenToGif.Domain

                presetsChild.NameSpace = presetsChild.NameSpace.Replace("ScreenToGif.ViewModel.ExportPresets", "ScreenToGif.Domain.Models.Preset.Export")
                    .Replace("assembly=ScreenToGif.ViewModel", "assembly=ScreenToGif.Domain");

                if (presetsChild.Type == "SystemGifPreset" || presetsChild.Type == "GifskiGifPreset" || presetsChild.Type == "KGySoftGifPreset")
                    presetsChild.Attributes.RemoveAll(r => r.Key == "UseGlobalColorTable");

                presetsChild.Attributes.RemoveAll(r => r.Key == "DefaultExtension" ||
                                                       r.Key == "IsEncoderExpanded" ||
                                                       r.Key == "IsEncoderOptionsExpanded" ||
                                                       r.Key == "IsExportOptionsExpanded" ||
                                                       r.Key == "IsPartialExportExpanded" ||
                                                       r.Key == "IsOutputExpanded" ||
                                                       r.Key == "IsUploadExpanded" ||
                                                       r.Key == "ExportPartially" ||
                                                       r.Key == "PartialExport");
            }
        }

        var uploadPreset = properties.FirstOrDefault(a => a.Key == "UploadPresets");

        if (uploadPreset != null)
        {
            foreach (var presetsChild in uploadPreset.Children)
            {
                //UploadPreset => Migrate namespaces
            }
        }
        
        //Update namespaces.
        //Remove settings.

        //Remove UserSettings.All.EditorExtendChrome;

        return true;
    }
}