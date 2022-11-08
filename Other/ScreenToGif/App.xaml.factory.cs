using ScreenToGif.Controls;
using ScreenToGif.Controls.Recorder;
using ScreenToGif.Dialogs;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Util;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel;
using ScreenToGif.Views;
using ScreenToGif.Views.Recorders;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ScreenToGif;

public partial class App
{
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

    internal static bool CanOpenRecorder(object sender)
    {
        return Current?.Windows.OfType<Window>().All(a => a is not BaseRecorder) ?? true;
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

    internal static void TryOpeningEditor(bool forGlobal = false)
    {
        if ((forGlobal && ViewModel.IgnoreHotKeys))
            return;

        OpenEditor(null);
    }

    internal static void OpenEditor(object parameter)
    {
        var startup = Current.Windows.OfType<Editor>().FirstOrDefault(f => !f.HasProjectLoaded);

        if (startup == null)
        {
            startup = new Editor();
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

    internal static bool CanOpenUpdater(object parameter)
    {
        //TODO: Get update info from view model.
        //return ViewModel.HasUpdate;
        return true;
    }

    internal static void OpenUpdater(object parameter)
    {
        //Try to install the update, closing the app if successful.
        if (InstallUpdate(true))
            Current.Shutdown(69);
    }

    internal static void TryOpeningOptions(bool forGlobal = false)
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

    internal static void TryOpeningFeedback(bool forGlobal = false)
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

    internal static void TryOpeningTroubleshooter(bool forGlobal = false)
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

    internal static void TrayLeftClick(object parameter)
    {
        Interact(UserSettings.All.LeftClickAction, UserSettings.All.LeftOpenWindow);
    }

    internal static void TrayLeftDoubleClick(object parameter)
    {
        Interact(UserSettings.All.DoubleLeftClickAction, UserSettings.All.DoubleLeftOpenWindow);
    }

    internal static void TrayMiddleClick(object parameter)
    {
        Interact(UserSettings.All.MiddleClickAction, UserSettings.All.MiddleOpenWindow);
    }
    
    internal void TryExiting(bool forGlobal = false)
    {
        if ((forGlobal && ViewModel.IgnoreHotKeys) || !CanExitApplication(null))
            return;

        ExitApplication(null);
    }

    internal static bool CanExitApplication(object parameter)
    {
        return Current?.Windows.OfType<BaseRecorder>().All(a => a.ViewModel.Stage != RecorderStages.Recording) ?? false;
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

    internal static async void CheckForUpdates(object parameter)
    {
        //When checking automatically (paramater = null).
        if (!UserSettings.All.CheckForUpdates && parameter == null)
            return;

#if FULL_MULTI_MSIX_STORE
            return;
#endif

        //If the app was installed by Chocolatey, avoid updating via normal means.
        if (await IsChocolateyPackage())
            return;

        //Try checking for the update on Github first then fallbacks to Fosshub.
        if (!await CheckOnGithub())
            await CheckOnFosshub();
    }

    //TODO: Move to other file.

    private static async Task<bool> IsChocolateyPackage()
    {
        try
        {
            //Binaries distributed via Chocolatey are of Installer or Portable types.
            if (IdentityHelper.ApplicationType != ApplicationTypes.FullSingle && IdentityHelper.ApplicationType != ApplicationTypes.DependantSingle)
                return false;

            //If Chocolatey is installed and ScreenToGif was installed via its service, it will be listed.
            var choco = await ProcessHelper.Start("choco list -l screentogif");

            if (!choco.Contains("screentogif"))
                return false;

            //The Portable package gets shimmed when installing via choco.
            //As for the Installer package, I'm letting it to be updated via normal means too (for now).
            var shim = await ProcessHelper.Start("$a='path to executable: '; (ScreenToGif.exe --shimgen-noop | Select-String $a) -split $a | ForEach-Object Trim");
            var path = ProcessHelper.GetEntryAssemblyPath();

            return shim.Contains(path);
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Not possible to detect Chocolatey package.");
            return false;
        }
    }

    private static async Task<bool> CheckOnGithub()
    {
        try
        {
            #region GraphQL equivalent

            //query {
            //    repository(owner: "NickeManarin", name: "ScreenToGif") {
            //        releases(first: 1, orderBy: { field: CREATED_AT, direction: DESC}) {
            //            nodes {
            //                name
            //                tagName
            //                createdAt
            //                url
            //                isPrerelease
            //                description
            //                releaseAssets(last: 2) {
            //                    nodes {
            //                        name
            //                        downloadCount
            //                        downloadUrl
            //                        size
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            #endregion

            var proxy = WebHelper.GetProxy();
            var handler = new HttpClientHandler
            {
                Proxy = proxy,
                UseProxy = proxy != null
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393");
            using var response = await client.GetAsync("https://api.github.com/repos/NickeManarin/ScreenToGif/releases/latest");
            var result = await response.Content.ReadAsStringAsync();

            var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(result), new System.Xml.XmlDictionaryReaderQuotas());
            var release = XElement.Load(jsonReader);

            var version = Version.Parse(release.XPathSelectElement("tag_name")?.Value ?? "0.1");

            //if (version.Major == 0 || version <= Assembly.GetExecutingAssembly().GetName().Version)
            //    return true;

            ParseDownloadUrls(release, version);

            //Download update to be installed when the app closes.
            if (UserSettings.All.InstallUpdates && ViewModel.UpdaterViewModel.HasDownloadLink)
                await DownloadUpdate();

            return true;
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Impossible to check for updates on Github");
            return false;
        }
        finally
        {
            GC.Collect();
        }
    }

    private static bool ParseDownloadUrls(XElement release, Version version, bool fromGithub = true)
    {
        var moniker = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            _ => "arm64"
        };

        switch (IdentityHelper.ApplicationType)
        {
            case ApplicationTypes.FullMultiMsix:
            {
                //Only get Msix files.
                //ScreenToGif.2.36.Package.x64.msix
                //ScreenToGif.2.36.Package.msix

                var package = (release.Element("assets") ?? release.Element("items"))?.Elements().FirstOrDefault(f =>
                {
                    var name = (f.Element("name")?.Value ?? f.Element("title")?.Value ?? "").ToLower();

                    return name.EndsWith(".package." + moniker + ".msix") || name.EndsWith("package.msix");
                });

                return SetDownloadDetails(fromGithub, version, release, package);
            }
            case ApplicationTypes.DependantSingle:
            {
                //Get portable or installer packages, light or not.
                //ScreenToGif.2.36.Light.Portable.x64.zip
                //ScreenToGif.2.36.Light.Portable.zip
                //Or
                //ScreenToGif.2.36.Light.Setup.x64.msi
                //ScreenToGif.2.36.Light.Setup.msi

                var portable = (release.Element("assets") ?? release.Element("items"))?.Elements().FirstOrDefault(f =>
                {
                    var name = (f.Element("name")?.Value ?? f.Element("title")?.Value ?? "").ToLower();

                    return name.EndsWith(".light.portable." + moniker + ".zip") || name.EndsWith(".light.portable.zip");
                });
                var installer = (release.Element("assets") ?? release.Element("items"))?.Elements().FirstOrDefault(f =>
                {
                    var name = (f.Element("name")?.Value ?? f.Element("title")?.Value ?? "").ToLower();

                    return name.EndsWith(".light.setup." + moniker + ".msi") || name.EndsWith(".light.setup.msi");
                });

                //If missing light (framework dependent) variant, download full package.
                if (installer == null)
                {
                    portable = (release.Element("assets") ?? release.Element("items"))?.Elements().FirstOrDefault(f =>
                    {
                        var name = (f.Element("name")?.Value ?? f.Element("title")?.Value ?? "").ToLower();

                        return name.EndsWith(".portable." + moniker + ".zip") || name.EndsWith(".portable.zip");
                    });
                    installer = (release.Element("assets") ?? release.Element("items"))?.Elements().FirstOrDefault(f =>
                    {
                        var name = (f.Element("name")?.Value ?? f.Element("title")?.Value ?? "").ToLower();

                        return name.EndsWith(".setup." + moniker + ".msi") || name.EndsWith(".setup.msi");
                    });
                }

                return SetDownloadDetails(fromGithub, version, release, installer, portable);
            }
            default:
            {
                //Get portable or installer packages, light or not.
                //ScreenToGif.2.36.Portable.x64.zip
                //ScreenToGif.2.36.Portable.zip
                //Or
                //ScreenToGif.2.36.Setup.x64.msi
                //ScreenToGif.2.36.Setup.msi

                var portable = (release.Element("assets") ?? release.Element("items"))?.Elements().FirstOrDefault(f =>
                {
                    var name = (f.Element("name")?.Value ?? f.Element("title")?.Value ?? "").ToLower();

                    return (name.EndsWith(".portable." + moniker + ".zip") || name.EndsWith("portable.zip")) && !name.Contains(".light.");
                });
                var installer = (release.Element("assets") ?? release.Element("items"))?.Elements().FirstOrDefault(f =>
                {
                    var name = (f.Element("name")?.Value ?? f.Element("title")?.Value ?? "").ToLower();

                    return (name.EndsWith(".setup." + moniker + ".msi") || name.EndsWith("setup.msi")) && !name.Contains(".light.");
                });

                return SetDownloadDetails(fromGithub, version, release, installer, portable);
            }
        }
    }

    private static bool SetDownloadDetails(bool fromGithub, Version version, XElement release, XElement installer, XElement portable = null)
    {
        if (installer == null)
        {
            ViewModel.UpdaterViewModel = new UpdaterViewModel
            {
                IsFromGithub = fromGithub,
                Version = version,
                Description = release.XPathSelectElement("body")?.Value ?? "",
                MustDownloadManually = true
            };

            return false;
        }

        if (fromGithub)
        {
            ViewModel.UpdaterViewModel = new UpdaterViewModel
            {
                Version = version,
                Description = release.XPathSelectElement("body")?.Value ?? "",

                PortableDownloadUrl = portable?.Element("browser_download_url")?.Value ?? "",
                PortableSize = Convert.ToInt64(portable?.Element("size")?.Value ?? "0"),
                PortableName = portable?.Element("name")?.Value ?? "ScreenToGif.zip",

                InstallerDownloadUrl = installer.Element("browser_download_url")?.Value ?? "",
                InstallerSize = Convert.ToInt64(installer.Element("size")?.Value ?? "0"),
                InstallerName = installer.Element("name")?.Value ?? "ScreenToGif.Setup.msi"
            };

            return true;
        }

        ViewModel.UpdaterViewModel = new UpdaterViewModel
        {
            IsFromGithub = false,
            Version = version,
            PortableDownloadUrl = portable?.Element("link")?.Value ?? "",
            InstallerDownloadUrl = installer.Element("link")?.Value ?? "",
        };

        return true;
    }

    private static async Task CheckOnFosshub()
    {
        try
        {
            var proxy = WebHelper.GetProxy();
            var handler = new HttpClientHandler
            {
                Proxy = proxy,
                UseProxy = proxy != null,
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393");
            using var response = await client.GetAsync("https://www.fosshub.com/feed/5bfc6fce8c9fe8186f809d24.json");
            var result = await response.Content.ReadAsStringAsync();

            var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(result), new System.Xml.XmlDictionaryReaderQuotas());
            var release = XElement.Load(jsonReader);

            var version = Version.Parse(release.XPathSelectElement("release/items")?.FirstNode?.XPathSelectElement("version")?.Value ?? "0.1");

            if (version.Major == 0 || version <= Assembly.GetExecutingAssembly().GetName().Version)
                return;

            ParseDownloadUrls(release, version);
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Impossible to check for updates on Fosshub");
        }
        finally
        {
            GC.Collect();
        }
    }

    internal static async Task<bool> DownloadUpdate()
    {
        try
        {
            lock (UserSettings.Lock)
            {
                if (string.IsNullOrWhiteSpace(UserSettings.All.TemporaryFolderResolved) || ViewModel.UpdaterViewModel.IsDownloading)
                    return false;

                var folder = Path.Combine(UserSettings.All.TemporaryFolderResolved, "ScreenToGif", "Updates");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                ViewModel.UpdaterViewModel.ActivePath = Path.Combine(folder, ViewModel.UpdaterViewModel.ActiveName);

                //Check if installer was already downloaded.
                if (File.Exists(ViewModel.UpdaterViewModel.ActivePath))
                {
                    //Minor issue, if for some reason, the update has the exact same size, this won't work properly. I would need to check a hash.
                    if (GetSize(ViewModel.UpdaterViewModel.ActivePath) == ViewModel.UpdaterViewModel.ActiveSize)
                        return false;

                    File.Delete(ViewModel.UpdaterViewModel.ActivePath);
                }

                ViewModel.UpdaterViewModel.IsDownloading = true;
            }

            var proxy = WebHelper.GetProxy();
            var handler = new HttpClientHandler
            {
                Proxy = proxy,
                UseProxy = proxy != null,
            };

            //TODO: Use HttpClientFactory
            //https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            //https://marcominerva.wordpress.com/2019/03/13/using-httpclientfactory-with-wpf-on-net-core-3-0/

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393");

                var response = await client.GetAsync(ViewModel.UpdaterViewModel.ActiveDownloadUrl);

                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var fileInfo = new FileInfo(ViewModel.UpdaterViewModel.ActivePath);
                    await using var fileStream = fileInfo.OpenWrite();
                    await stream.CopyToAsync(fileStream);
                }
                else
                {
                    throw new FileNotFoundException("Impossible to download update.");
                }
            }

            ViewModel.UpdaterViewModel.MustDownloadManually = false;
            ViewModel.UpdaterViewModel.TaskCompletionSource?.TrySetResult(true);
            return true;
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Impossible to automatically download update");
            ViewModel.UpdaterViewModel.MustDownloadManually = true;
            ViewModel.UpdaterViewModel.TaskCompletionSource?.TrySetResult(false);
            return false;
        }
        finally
        {
            ViewModel.UpdaterViewModel.IsDownloading = false;
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

    //Maybe turn into a helper
    private static long GetSize(string path)
    {
        var info = new FileInfo(path);
        info.Refresh();

        return info.Length;
    }

    private static async Task RecorderCallback(Window caller, Editor editor, object sender, EventArgs args)
    {
        var window = sender as BaseRecorder;

        if (window?.Project != null && window.Project.Any)
        {
            if (editor == null)
            {
                if (UserSettings.All.WindowAfterRecording == AfterRecordingWindows.SaveDialog)
                    ShowExporter(window.Project);
                else
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
        var editor = Current.Windows.OfType<Editor>().FirstOrDefault(f => !f.HasProjectLoaded);

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

    private static void ShowExporter(RecordingProject project = null)
    {
        var exporter = new Exporter();
        exporter.Closed += (_, _) => CloseOrNot();
        exporter.Show();

        exporter.LoadProject(project);

        Current.MainWindow = exporter;
        exporter.Activate();
    }

    private static void Interact(NotificationIconActions action, StartupWindows open)
    {
        switch (action)
        {
            case NotificationIconActions.OpenWindow:
            {
                switch (open)
                {
                    case StartupWindows.Welcome:
                    {
                        OpenStartup(null);
                        break;
                    }
                    case StartupWindows.ScreenRecorder:
                    {
                        TryOpeningScreenRecorder();
                        return;
                    }
                    case StartupWindows.WebcamRecorder:
                    {
                        TryOpeningWebcamRecorder();
                        break;
                    }
                    case StartupWindows.SketchboardRecorder:
                    {
                        TryOpeningBoardRecorder();
                        break;
                    }
                    case StartupWindows.Editor:
                    {
                        TryOpeningEditor();
                        break;
                    }
                }

                break;
            }

            case NotificationIconActions.ToggleWindows:
            {
                var all = Current.Windows.OfType<Window>().Where(w => w.Content != null).ToList();

                if (all.Count == 0)
                {
                    Interact(NotificationIconActions.OpenWindow, open);
                    return;
                }

                if (all.Any(n => n.WindowState != WindowState.Minimized))
                {
                    //Minimize all windows, disabling before to prevent some behaviors.
                    foreach (var f in all)
                        f.IsEnabled = false;

                    foreach (var f in all)
                        f.WindowState = WindowState.Minimized;

                    foreach (var f in all)
                        f.IsEnabled = true;
                }
                else
                {
                    //Restore all windows.
                    foreach (var window in all)
                        window.WindowState = WindowState.Normal;
                }

                break;
            }

            case NotificationIconActions.MinimizeWindows:
            {
                var all = Current.Windows.OfType<Window>().Where(w => w.Content != null).ToList();

                if (all.Count == 0)
                {
                    Interact(NotificationIconActions.OpenWindow, open);
                    return;
                }

                foreach (var window in all)
                    window.WindowState = WindowState.Minimized;

                break;
            }

            case NotificationIconActions.MaximizeWindows:
            {
                var all = Current.Windows.OfType<Window>().Where(w => w.Content != null).ToList();

                if (all.Count == 0)
                {
                    Interact(NotificationIconActions.OpenWindow, open);
                    return;
                }

                foreach (var window in all)
                    window.WindowState = WindowState.Normal;

                break;
            }
        }
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
}