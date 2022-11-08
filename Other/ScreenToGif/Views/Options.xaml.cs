using ScreenToGif.Controls;

namespace ScreenToGif.Views;

public partial class Options : ExWindow
{
    internal const int ApplicationIndex = 0;
    internal const int RecorderIndex = 1;

    internal const int InterfaceIndex = 2;
    internal const int AutomatedTasksIndex = 3;
    internal const int ShortcutsIndex = 4;
    internal const int LanguageIndex = 5;
    internal const int TempFilesIndex = 6;
    internal const int UploadIndex = 7;
    internal const int ExtrasIndex = 8;
    internal const int DonateIndex = 9;
    internal const int AboutIndex = 10; 

    //Sections:
    //  Application
    //      Startup, Language, Theme
    //  Screen Recorder
    //      Frequency, mode, remeber stuff
    //  Webcam recorder
    //  Sketchboard recorder
    //  Export window?
    //      IDK
    //  Editor
    //      Behavior
    //  Shortcuts
    //  Automated tasks?
    //      Maybe separate by recording type?
    //  Storage
    //  Cloud
    //  Plugins
    //  About/Update/Licensing?

    public Options(int tab = 0)
    {
        InitializeComponent();
    }

    public void SelectTab(int tab)
    {

    }
}