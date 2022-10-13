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
        //Startup -> FirstWindow
        /// 0: Screen recorder
        /// 1: Webcam recorder
        /// 2: Sketchboard recorder
        /// 3: Editor
        /// 4: Welcome
        /// 5: Nothing


        //Update namespaces.
        //Remove settings.

        //Remove UserSettings.All.EditorExtendChrome;

        return true;
    }
}