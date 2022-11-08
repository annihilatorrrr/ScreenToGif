using ScreenToGif.Controls;
using ScreenToGif.Util;
using ScreenToGif.Util.Native;
using ScreenToGif.Util.Settings;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenToGif.Dialogs;

public partial class DownloadDialog : ExWindow
{
    public bool WasPromptedManually { get; set; }

    public bool RunAfterwards { get; set; }

    public DownloadDialog()
    {
        InitializeComponent();

        DataContext = App.ViewModel.UpdaterViewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (App.ViewModel.UpdaterViewModel == null)
        {
            InfoBar.Warning(LocalizationHelper.Get("S.Updater.NoUpdate"), LocalizationHelper.Get("S.Updater.NoUpdate.Info"));
            return;
        }

        VersionCardGrid.Visibility = Visibility.Visible;

        //If the download failed or was preemptively detected as not possible to automate.
        if (App.ViewModel.UpdaterViewModel.MustDownloadManually)
            InfoBar.Warning(LocalizationHelper.Get("S.Updater.Unsupported"), LocalizationHelper.Get("S.Updater.Unsupported.Info"));

        try
        {
            //Detect if this is portable or installed. Download the proper file.
            App.ViewModel.UpdaterViewModel.IsInstaller = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory).Any(x => x.ToLowerInvariant().EndsWith("screentogif.visualelementsmanifest.xml"));

            //Details.
            if (!App.ViewModel.UpdaterViewModel.IsFromGithub)
                MarkdownScrollViewer.Markdown = LocalizationHelper.Get("S.Updater.Info.NewVersionAvailable");
            
            //If set to force the download the portable version of the app, check if it was downloaded.
            if (UserSettings.All.PortableUpdate)
            {
                //If the update was already downloaded.
                if (File.Exists(App.ViewModel.UpdaterViewModel.PortablePath))
                {
                    //If it's still downloading, wait for it to finish before displaying "Open".
                    if (App.ViewModel.UpdaterViewModel.IsDownloading)
                    {
                        App.ViewModel.UpdaterViewModel.TaskCompletionSource = new TaskCompletionSource<bool>();
                        await App.ViewModel.UpdaterViewModel.TaskCompletionSource.Task;

                        if (!IsLoaded)
                            return;
                    }

                    UpdateButton.SetResourceReference(ContentProperty, "S.Updater.InstallManually");
                }

                return;
            }

            //If set to download automatically, check if the installer was downloaded.
            if (UserSettings.All.InstallUpdates)
            {
                //If the update was already downloaded.
                if (File.Exists(App.ViewModel.UpdaterViewModel.InstallerPath))
                {
                    //If it's still downloading, wait for it to finish before displaying "Install".
                    if (App.ViewModel.UpdaterViewModel.IsDownloading)
                    {
                        App.ViewModel.UpdaterViewModel.TaskCompletionSource = new TaskCompletionSource<bool>();
                        await App.ViewModel.UpdaterViewModel.TaskCompletionSource.Task;

                        if (!IsLoaded)
                            return;
                    }

                    UpdateButton.SetResourceReference(ContentProperty, "S.Updater.Install");

                    //When the update was prompted manually, the user can set the installer to run the app afterwards.
                    if (WasPromptedManually)
                    {
                        RunAfterDownloadCheckBox.Visibility = Visibility.Visible;
                        RunAfterDownloadCheckBox.IsChecked = true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Impossible to load the download details");
            InfoBar.Error("Error", LocalizationHelper.Get("S.Updater.Warning.Show"));
        }
        finally
        {
            SizeToContent = SizeToContent.Height;
            Height = ActualHeight;
            SizeToContent = SizeToContent.Manual;
            
            CenterOnScreen();
        }
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        InfoBar.Hide();

        if (App.ViewModel.UpdaterViewModel.MustDownloadManually)
        {
            ProcessHelper.StartWithShell("https://www.screentogif.com");
            DialogResult = false;
            return;
        }

        //TODO: Is this still necessary?
        //if (EncodingManager.Encodings.Any(a => a.Status == EncodingStatus.Processing))
        //{
        //    InfoBar.Warning(LocalizationHelper.Get("S.Updater.Warning.Encoding"));
        //    return;
        //}

        UpdateButton.IsEnabled = false;
        InfoBar.Info("", LocalizationHelper.Get("S.Updater.Downloading"));

        RunAfterwards = RunAfterDownloadCheckBox.IsChecked == true;

        //If it's still downloading, wait for it to finish.
        if (App.ViewModel.UpdaterViewModel.IsDownloading)
        {
            App.ViewModel.UpdaterViewModel.TaskCompletionSource = new TaskCompletionSource<bool>();
            await App.ViewModel.UpdaterViewModel.TaskCompletionSource.Task;

            if (!IsLoaded)
                return;
        }

        //If update already downloaded, simply close this window. The installation will happen afterwards.
        if (File.Exists(App.ViewModel.UpdaterViewModel.ActivePath))
        {
            GC.Collect();
            DialogResult = true;
            return;
        }

        //When the update was not queried from Github, the download must be done by browser.
        if (!App.ViewModel.UpdaterViewModel.IsFromGithub)
        {
            try
            {
                ProcessHelper.StartWithShell(App.ViewModel.UpdaterViewModel.ActiveDownloadUrl);
            }
            catch (Exception ex)
            {
                LogWriter.Log(ex, "Impossible to open the browser to download the update.", App.ViewModel.UpdaterViewModel?.ActiveDownloadUrl);
            }

            GC.Collect();
            DialogResult = true;
            return;
        }

        ProgressBar.Visibility = Visibility.Visible;
        RunAfterDownloadCheckBox.Visibility = Visibility.Collapsed;

        var result = await App.DownloadUpdate();

        //If cancelled.
        if (!IsLoaded)
            return;

        if (!result)
        {
            UpdateButton.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Hidden;
            InfoBar.Error("", LocalizationHelper.Get("S.Updater.Warning.Download"));
            return;
        }

        //If the update was downloaded successfully, close this window to run.
        if (File.Exists(App.ViewModel.UpdaterViewModel.ActivePath))
        {
            GC.Collect();
            InfoBar.Hide();
            DialogResult = true;
            return;
        }

        InfoBar.Error("", LocalizationHelper.Get("S.Updater.Warning.Download"));
    }

    private void DismissButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        App.ViewModel.UpdaterViewModel.TaskCompletionSource = null;
    }

    private void CenterOnScreen()
    {
        //Since the list of monitors could have been changed, it needs to be queried again.
        var monitors = MonitorHelper.AllMonitorsGranular();

        //Detect closest screen to the point (previously selected top/left point or current mouse coordinate).
        var point = new Point((int)Left, (int)Top);
        var closest = monitors.FirstOrDefault(x => x.Bounds.Contains(point)) ?? monitors.FirstOrDefault(x => x.IsPrimary) ?? monitors.FirstOrDefault();

        if (closest == null)
            throw new Exception("It was not possible to get a list of known screens.");

        //Move the window to the correct location.
        Left = closest.WorkingArea.Left + closest.WorkingArea.Width / 2d - ActualWidth / 2d;
        Top = closest.WorkingArea.Top + closest.WorkingArea.Height / 2d - ActualHeight / 2d;
    }
}