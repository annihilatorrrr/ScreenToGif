using ScreenToGif.Controls.Recorder;
using ScreenToGif.Dialogs;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Util;
using ScreenToGif.Util.Settings;
using ScreenToGif.Views;
using ScreenToGif.Views.Recorders;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenToGif;

public partial class App
{
    internal static bool CanOpenRecorder(object sender)
    {
        return Current?.Windows.OfType<Window>().All(a => a is not BaseRecorderOld) ?? true;
    }

    internal static void TryOpeningScreenRecorder(bool forGlobal = false)
    {
        if ((forGlobal && ViewModel.IgnoreHotKeys) || !CanOpenRecorder(null))
            return;

        OpenScreenRecorder(null);
    }

    internal static void OpenScreenRecorder(object parameter)
    {
        var recorder = Current.Windows.OfType<ScreenRecorder>().FirstOrDefault();
        var caller = parameter as Window;
        var editor = parameter as Editor;

        if (editor == null)
            caller?.Hide();

        if (recorder == null)
        {
            recorder = new ScreenRecorder();
            recorder.Closed += async (sender, args) => await RecorderCallback(caller, editor, sender, args);

            Current.MainWindow = recorder;
            recorder.Show();
        }
        else
        {
            if (recorder.WindowState == WindowState.Minimized)
                recorder.WindowState = WindowState.Normal;

            Current.MainWindow = recorder;
            recorder.Activate();
        }
    }

    internal static void TryOpeningWebcamRecorder(bool forGlobal = false)
    {
        if ((forGlobal && ViewModel.IgnoreHotKeys) || !CanOpenRecorder(null))
            return;

        OpenWebcamRecorder(null);
    }

    internal static void OpenWebcamRecorder(object parameter)
    {
        //Open Recorder, wait for callback.
        ErrorDialog.ShowStatic("Webcam Recorder", "Not yet implemented.");
    }

    internal static void TryOpeningBoardRecorder(bool forGlobal = false)
    {
        if ((forGlobal && ViewModel.IgnoreHotKeys) || !CanOpenRecorder(null))
            return;

        OpenBoardRecorder(null);
    }

    internal static void OpenBoardRecorder(object parameter)
    {
        var recorder = Current.Windows.OfType<SketchboardRecorder>().FirstOrDefault();

        if (recorder == null)
        {
            recorder = new SketchboardRecorder();
            recorder.Closed += (_, _) => CloseOrNot();
            //Open Recorder, wait for callback.

            recorder.Show();
        }
        else
        {
            if (recorder.WindowState == WindowState.Minimized)
                recorder.WindowState = WindowState.Normal;

            recorder.Activate();
        }
    }

    internal static void Launch(object paramater)
    {
        switch (paramater)
        {
            case StartupWindows.None:
                return;

            case StartupWindows.ScreenRecorder:
            {
                TryOpeningScreenRecorder();
                return;
            }

            case StartupWindows.WebcamRecorder:
            {
                TryOpeningWebcamRecorder();
                return;
            }

            case StartupWindows.SketchboardRecorder:
            {
                TryOpeningBoardRecorder();
                return;
            }

            case StartupWindows.Editor:
            {
                OpenEditor(null);
                return;
            }
            
            default:
            {
                if (UserSettings.All.IsFirstTime)
                {
                    if (SetupDialog.Ask())
                    {
                        UserSettings.All.IsFirstTime = false;

                        Launch(UserSettings.All.StartupWindow);
                        return;
                    }
                }

                OpenStartup(null);
                return;
            }
        }
    }

    internal static void OpenStartup(object parameter)
    {
        var startup = Current.Windows.OfType<Startup>().FirstOrDefault();

        if (startup == null)
        {
            startup = new Startup();
            startup.Closed += (_, _) => CloseOrNot();

            startup.Show();
        }
        else
        {
            if (startup.WindowState == WindowState.Minimized)
                startup.WindowState = WindowState.Normal;

            startup.Activate();
        }
    }

    internal void TryOpeningEditor(bool forGlobal = false)
    {
        if ((forGlobal && ViewModel.IgnoreHotKeys))
            return;

        OpenEditor(null);
    }

    internal static void OpenEditor(object parameter)
    {

    }

    internal static bool CanOpenUpdater(object parameter)
    {
        //TODO: Get update info from view model.
        //return ViewModel.HasUpdate;
        return true;
    }

    internal static void OpenUpdater(object parameter)
    {

    }

    internal void TryOpeningOptions(bool forGlobal = false)
    {
        if ((forGlobal && ViewModel.IgnoreHotKeys))
            return;

        OpenOptions(null);
    }

    internal static void OpenOptions(object parameter)
    {
        var options = Current.Windows.OfType<Options>().FirstOrDefault();
        var tab = parameter as int? ?? 0; //Parameter that selects which tab to be displayed.

        if (options == null)
        {
            options = new Options(tab);
            options.Closed += (_, _) => CloseOrNot();
            options.Show();
        }
        else
        {
            if (options.WindowState == WindowState.Minimized)
                options.WindowState = WindowState.Normal;

            options.SelectTab(tab);
            options.Activate();
        }
    }

    internal void TryOpeningFeedback(bool forGlobal = false)
    {
        if ((forGlobal && ViewModel.IgnoreHotKeys))
            return;

        OpenFeedback(null);
    }

    internal static void OpenFeedback(object parameter)
    {
        //var feedback = Current.Windows.OfType<Feedback>().FirstOrDefault();
        //var tab = parameter as int? ?? 0; //Parameter that selects which tab to be displayed.

        //if (feedback == null)
        //{
        //    feedback = new Feedback(tab);
        //    feedback.Closed += (_, _) => CloseOrNot();
        //    feedback.Show();
        //}
        //else
        //{
        //    if (feedback.WindowState == WindowState.Minimized)
        //        feedback.WindowState = WindowState.Normal;

        //    feedback.Activate();
        //}
    }

    internal void TryOpeningTroubleshooter(bool forGlobal = false)
    {
        if ((forGlobal && ViewModel.IgnoreHotKeys))
            return;

        OpenTroubleshooter(null);
    }

    internal static void OpenTroubleshooter(object parameter)
    {
        //var feedback = Current.Windows.OfType<Troubleshooter>().FirstOrDefault();
        //var tab = parameter as int? ?? 0; //Parameter that selects which tab to be displayed.

        //if (feedback == null)
        //{
        //    feedback = new Troubleshooter(tab);
        //    feedback.Closed += (_, _) => CloseOrNot();
        //    feedback.Show();
        //}
        //else
        //{
        //    if (feedback.WindowState == WindowState.Minimized)
        //        feedback.WindowState = WindowState.Normal;

        //    feedback.Activate();
        //}
    }

    internal void TryExiting(bool forGlobal = false)
    {
        if ((forGlobal && ViewModel.IgnoreHotKeys) || !CanExitApplication(null))
            return;

        ExitApplication(null);
    }

    internal static bool CanExitApplication(object parameter)
    {
        return Current?.Windows.OfType<BaseRecorderOld>().All(a => a.Stage != RecorderStages.Recording) ?? false;
    }

    internal static void ExitApplication(object parameter)
    {
        //if (UserSettings.All.NotifyWhileClosingApp && !Dialog.Ask(LocalizationHelper.Get("S.Exiting.Title"), LocalizationHelper.Get("S.Exiting.Instruction"), LocalizationHelper.Get("S.Exiting.Message")))
        //    return;

        if (UserSettings.All.DeleteCacheWhenClosing)
            StorageHelper.PurgeCache();

        Current.Shutdown(69);
    }

    internal static async void ClearCache(object parameter)
    {
        await Task.Factory.StartNew(() =>
        {
            //Run if: Not already running (Check outside of here, if configured to run)
            //Use StorageHelper methods.
            //Update viewModel.

            try
            {
                if (string.IsNullOrWhiteSpace(UserSettings.All.TemporaryFolderResolved))
                    return;

                ViewModel.IsClearingCache = true;

                StorageHelper.PurgeCache(UserSettings.All.AutomaticCleanUpDays);
                
                //Clear updates cache.
            }
            catch (Exception ex)
            {
                LogWriter.Log(ex, "Cache clean-up task");
            }
            finally
            {
                ViewModel.IsClearingCache = false;

                //Check disk space.
            }

            //App.ViewModel.IsClearingCache = true;

            //try
            //{
            //    if (!UserSettings.All.AutomaticCleanUp || Global.IsCurrentlyDeletingFiles || string.IsNullOrWhiteSpace(UserSettings.All.TemporaryFolderResolved))
            //        return;

            //    Global.IsCurrentlyDeletingFiles = true;

            //    ClearRecordingCache();
            //    ClearUpdateCache();
            //}
            //catch (Exception ex)
            //{
            //    LogWriter.Log(ex, "Automatic clean up");
            //}
            //finally
            //{
            //    Global.IsCurrentlyDeletingFiles = false;
            //    CheckDiskSpace();
            //}

        }, TaskCreationOptions.LongRunning);
    }

    //TODO: Move to other file.
    private static async Task RecorderCallback(Window caller, Editor editor, object sender, EventArgs args)
    {
        var window = sender as BaseRecorderOld;

        if (window?.Project != null && window.Project.Any)
        {
            if (editor == null)
            {
                await ShowEditor(window.Project);
                caller?.Close();
                return;
            }

            await editor.LoadProject(window.Project);
            return;
        }

        if (editor == null)
        {
            caller?.Show();
            CloseOrNot();
            return;
        }

        await editor.LoadProject(null);
    }

    private static async Task ShowEditor(RecordingProject project = null, bool openMedia = false)
    {
        var editor = Current.Windows.OfType<Editor>().FirstOrDefault(f => f.HasProjectLoaded);

        if (editor == null)
        {
            editor = new Editor();
            editor.Closed += (_, _) => CloseOrNot();
            editor.Show();
        }
        else
        {
            //TODO: Detect if the last state was normal/maximized.
            if (editor.WindowState == WindowState.Minimized)
                editor.WindowState = WindowState.Normal;
        }

        if (project != null)
            await editor.LoadProject(project);
        else if (openMedia)
            editor.LoadFromArguments();

        Current.MainWindow = editor;
        editor.Activate();
    }

    private static void CloseOrNot()
    {
        //When closed, check if it's the last window, then close if it's the configured behavior.
        if (UserSettings.All.ShowNotificationIcon && UserSettings.All.KeepOpen)
            return;

        //We only need to check loaded windows that have content, since any special window could be open.
        if (Current.Windows.Cast<Window>().Count(window => window.HasContent) == 0)
        {
            //Install the available update on closing.
            if (UserSettings.All.InstallUpdates)
                InstallUpdate();

            if (UserSettings.All.DeleteCacheWhenClosing)
            {
                //TODO: Create cache dialog.
                //if (UserSettings.All.AskDeleteCacheWhenClosing && !CacheDialog.Ask(false, out _))
                //    return;

                StorageHelper.PurgeCache();
            }

            Current.Shutdown(2);
        }
    }

    internal static bool InstallUpdate(bool wasPromptedManually = false)
    {
        try
        {
            //No new release available.
            if (ViewModel.UpdaterViewModel == null)
                return false;

            //TODO: Check if Windows is not turning off.

            var runAfterwards = false;

            //Prompt if:
            //Not configured to download the update automatically OR
            //Configured to download but set to prompt anyway OR
            //Update binary detection failed (manual update required) OR
            //Download not completed (perharps because the notification was triggered by a query on Fosshub).
            if (UserSettings.All.PromptToInstall || !UserSettings.All.InstallUpdates || string.IsNullOrWhiteSpace(ViewModel.UpdaterViewModel.ActivePath) || ViewModel.UpdaterViewModel.MustDownloadManually)
            {
                //TODO: Download dialog.
                var download = new DownloadDialog { WasPromptedManually = wasPromptedManually };
                var result = download.ShowDialog();

                if (!result.HasValue || !result.Value)
                    return false;

                runAfterwards = download.RunAfterwards;
            }

            //Only try to install if the update was downloaded.
            if (!File.Exists(ViewModel.UpdaterViewModel.ActivePath))
                return false;

            if (UserSettings.All.PortableUpdate || IdentityHelper.ApplicationType == ApplicationTypes.FullMultiMsix)
            {
                //In portable or Msix mode, simply open the zip/msix file and close ScreenToGif.
                ProcessHelper.StartWithShell(ViewModel.UpdaterViewModel.ActivePath);
                return true;
            }

            //Detect installed components.
            var files = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory).ToList();
            var isInstaller = files.Any(x => x.ToLowerInvariant().EndsWith("screentogif.visualelementsmanifest.xml"));
            var hasGifski = files.Any(x => x.ToLowerInvariant().EndsWith("gifski.dll"));
            var hasDesktopShortcut = File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ScreenToGif.lnk")) ||
                                     File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "ScreenToGif.lnk"));
            var hasMenuShortcut = File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Windows", "Start Menu", "Programs", "ScreenToGif.lnk")) ||
                                  File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft", "Windows", "Start Menu", "Programs", "ScreenToGif.lnk"));

            //MsiExec does not like relative paths.
            var isRelative = !string.IsNullOrWhiteSpace(ViewModel.UpdaterViewModel.InstallerPath) && !Path.IsPathRooted(ViewModel.UpdaterViewModel.InstallerPath);
            var nonRoot = isRelative ? Path.GetFullPath(ViewModel.UpdaterViewModel.InstallerPath) : ViewModel.UpdaterViewModel.InstallerPath;

            //msiexec /i PATH INSTALLDIR="" INSTALLAUTOMATICALLY=yes INSTALLPORTABLE=No ADDLOCAL=Binary
            //msiexec /a PATH TARGETDIR="" INSTALLAUTOMATICALLY=yes INSTALLPORTABLE=yes ADDLOCAL=Binary

            var startInfo = new ProcessStartInfo
            {
                FileName = "msiexec",
                Arguments = $" {(isInstaller ? "/i" : "/a")} \"{nonRoot}\"" +
                            $" {(isInstaller ? "INSTALLDIR" : "TARGETDIR")}=\"{AppDomain.CurrentDomain.BaseDirectory}\" INSTALLAUTOMATICALLY=yes INSTALLPORTABLE={(isInstaller ? "no" : "yes")}" +
                            $" ADDLOCAL=Binary{(isInstaller ? ",Auxiliar" : "")}{(hasGifski ? ",Gifski" : "")}" +
                            $" {(wasPromptedManually && runAfterwards ? "RUNAFTER=yes" : "")}" +
                            (isInstaller ? $" INSTALLDESKTOPSHORTCUT={(hasDesktopShortcut ? "yes" : "no")} INSTALLSHORTCUT={(hasMenuShortcut ? "yes" : "no")}" : ""),
                Verb = UserSettings.All.ForceUpdateAsAdmin ? "runas" : ""
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            return true;
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Impossible to automatically install update");

            //TODO: Localize
            ErrorDialog.ShowStatic("Update", "It was not possible to install the update.", ex);
            return false;
        }
    }
}