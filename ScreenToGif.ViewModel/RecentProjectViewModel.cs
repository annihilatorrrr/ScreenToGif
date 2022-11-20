using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Util;
using System.Text;
using System.Windows.Input;

namespace ScreenToGif.ViewModel;

public class RecentProjectViewModel : BaseViewModel
{
    private string _title;
    private RecentProjectTypes _type;
    private DateTime _creationDate;
    private string _version;
    private bool _couldBeIncompatible;
    private string _path;

    public string Title
    {
        get => _title;
        set
        {
            SetProperty(ref _title, value);

            OnPropertyChanged(nameof(Symbol));
        }
    }

    public RecentProjectTypes Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public DateTime CreationDate
    {
        get => _creationDate;
        set => SetProperty(ref _creationDate, value);
    }

    public string Version
    {
        get => _version;
        set => SetProperty(ref _version, value);
    }

    public bool CouldBeIncompatible
    {
        get => _couldBeIncompatible;
        set => SetProperty(ref _couldBeIncompatible, value);
    }

    public string Path
    {
        get => _path;
        set => SetProperty(ref _path, value);
    }

    public FluentSymbols Symbol => Type switch
    {
        RecentProjectTypes.Recording => FluentSymbols.Record,
        RecentProjectTypes.Project => FluentSymbols.DocumentFilled,
        _ => FluentSymbols.WarningFilled,
    };

    public RoutedUICommand ExportProjectCommand { get; set; }

    public RoutedUICommand EditProjectCommand { get; set; }

    public RoutedUICommand RemoveProjectCommand { get; set; }

    public static RecentProjectViewModel FromPath(string path, RoutedUICommand exportCommand, RoutedUICommand editCommand, RoutedUICommand removeCommand)
    {
        try
        {
            var propertiesPath = System.IO.Path.Combine(path, "Properties.cache");
            var oldProjectPath = System.IO.Path.Combine(path, "Project.json");

            if (File.Exists(propertiesPath))
            {
                using var readStream = new FileStream(propertiesPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var type = Encoding.ASCII.GetString(readStream.ReadBytes(4));
                var version = readStream.ReadUInt16();

                if (type == "stgR")
                    readStream.Position += 10;
                else
                    readStream.Position += 14;

                readStream.ReadPascalString();
                readStream.ReadPascalString();
                readStream.Position += 1;

                var dateInTicks = readStream.ReadInt64();
                var date = new DateTime(dateInTicks);

                return new RecentProjectViewModel
                {
                    Type = type == "stgR" ? RecentProjectTypes.Recording : RecentProjectTypes.Project,
                    CreationDate = date,
                    Version = "3.0",
                    CouldBeIncompatible = version != 1,
                    Path = path,
                    ExportProjectCommand = exportCommand,
                    EditProjectCommand = editCommand,
                    RemoveProjectCommand = removeCommand,
                };
            }

            if (File.Exists(oldProjectPath))
            {
                //TODO: Improve parsing, without having to read the entire file.
                //Parse JSON, or maybe just read the text.
                var date = File.ReadAllText(oldProjectPath).FindTextBetween("\\/", "\\/");

                return new RecentProjectViewModel
                {
                    Type = RecentProjectTypes.OldRecording,
                    CreationDate = date?.ConvertJsonStringToDateTime() ?? DateTime.MinValue,
                    Version = "2",
                    Path = path,
                    ExportProjectCommand = exportCommand,
                    EditProjectCommand = editCommand,
                    RemoveProjectCommand = removeCommand,
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Project parsing from path");

            return null;
        }
    }
}