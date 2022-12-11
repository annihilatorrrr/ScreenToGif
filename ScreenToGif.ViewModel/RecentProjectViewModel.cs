using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Util;
using ScreenToGif.Util.JsonConverters;
using System.Text;
using System.Text.Json;
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
        RecentProjectTypes.Project => FluentSymbols.DocumentFilled,
        RecentProjectTypes.Recording or RecentProjectTypes.OldRecording => FluentSymbols.Record,
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
                var type = Encoding.ASCII.GetString(readStream.ReadBytes(4)); //4 bytes.
                var version = readStream.ReadUInt16(); //2 bytes

                if (type == "stgR")
                {
                    readStream.Position += 10;
                    readStream.ReadPascalString();
                    readStream.ReadPascalString();
                    readStream.Position += 1;
                }
                else
                {
                    readStream.Position += 12;

                    var backgroundSize = readStream.ReadUInt32();
                    readStream.Position += backgroundSize;
                    readStream.Position += 2;

                    var appNameSize = readStream.ReadByte();
                    readStream.Position += appNameSize;

                    var appVersionSize = readStream.ReadByte();
                    readStream.Position += appVersionSize;
                    readStream.Position += 1;
                }

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
                using var fileStream = new FileStream(oldProjectPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var json = JsonDocument.Parse(fileStream);
                
                var deserializeOptions = new JsonSerializerOptions();
                deserializeOptions.Converters.Add(new UnixEpochDateTimeOffsetConverter());
                deserializeOptions.Converters.Add(new UnixEpochDateTimeConverter());

                var date = json.RootElement.GetProperty("CreationDate").Deserialize<DateTime>(deserializeOptions);

                return new RecentProjectViewModel
                {
                    Type = RecentProjectTypes.OldRecording,
                    CreationDate = date,
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