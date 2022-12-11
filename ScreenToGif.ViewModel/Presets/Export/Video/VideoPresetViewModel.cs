using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Export.Video;
using ScreenToGif.ViewModel.Presets.Export.Video.Codecs;
using System.Collections.ObjectModel;
using System.Windows;

namespace ScreenToGif.ViewModel.Presets.Export.Video;

public class VideoPresetViewModel : ExportPresetViewModel
{
    private VideoSettingsModes _settingsMode;
    private string _parameters;
    private ObservableCollection<VideoCodecViewModel> _filteredVideoCodecs;
    private VideoCodecs _videoCodec;
    private VideoCodecViewModel _selectedVideoCodec;
    private VideoCodecPresets _codecPreset;
    private HardwareAccelerationModes _hardwareAcceleration = HardwareAccelerationModes.Auto;
    private int _pass = 1;
    private bool _isVariableBitRate;
    private int? _constantRateFactor;
    private decimal _bitRate;
    private int _qualityLevel = 5;
    private RateUnits _bitRateUnit = RateUnits.Megabits;
    private decimal _minimumBitRate;
    private RateUnits _minimumBitRateUnit = RateUnits.Megabits;
    private decimal _maximumBitRate;
    private RateUnits _maximumBitRateUnit = RateUnits.Megabits;
    private decimal _rateControlBuffer;
    private RateUnits _rateControlBufferUnit = RateUnits.Megabits;
    private VideoPixelFormats _pixelFormat;
    private Framerates _framerate = Framerates.Auto;
    private decimal _customFramerate = 25M;
    private Vsyncs _vsync = Vsyncs.Passthrough;

    public VideoSettingsModes SettingsMode
    {
        get => _settingsMode;
        set
        {
            SetProperty(ref _settingsMode, value);

            OnPropertyChanged(nameof(NormalVisibility));
            OnPropertyChanged(nameof(AdvancedVisibility));
            OnPropertyChanged(nameof(CommandResolved));
        }
    }

    public Visibility NormalVisibility => SettingsMode == VideoSettingsModes.Normal ? Visibility.Visible : Visibility.Collapsed;

    public Visibility AdvancedVisibility => SettingsMode == VideoSettingsModes.Advanced ? Visibility.Visible : Visibility.Collapsed;

    public string Parameters
    {
        get => _parameters;
        set
        {
            SetProperty(ref _parameters, value);

            OnPropertyChanged(nameof(CommandResolved));
        }
    }

    public string CommandResolved
    {
        get
        {
            var command = "ffmpeg\n" + (Parameters ?? "").Replace("{I}", "-safe 0 -i - ").Replace("{O}", $"-y \"{ResolvedOutputPath}\"");

            if (command.Contains("-pass 2"))
            {
                command = command.Replace("-pass 2 ", $"-pass 1 -passlogfile -y \"[{ResolvedOutputPath}]\" ") +
                          Environment.NewLine + Environment.NewLine +
                          command.Replace("-pass 2", $"-pass 2 -passlogfile -y \"[{ResolvedOutputPath}]\" ");
            }

            return command;
        }
    }

    public ObservableCollection<VideoCodecViewModel> FilteredVideoCodecs
    {
        get
        {
            if (_filteredVideoCodecs == null)
                FilterVideoCodecs();

            return _filteredVideoCodecs;
        }
        set => SetProperty(ref _filteredVideoCodecs, value);
    }

    public VideoCodecs VideoCodec
    {
        get => _videoCodec;
        set
        {
            if (SetProperty(ref _videoCodec, value))
                SelectedVideoCodec = FilteredVideoCodecs.FirstOrDefault(f => f.Type == VideoCodec);
            
            OnPropertyChanged(nameof(ConstantRateFactorVisibility));
        }
    }

    public VideoCodecViewModel SelectedVideoCodec
    {
        get => _selectedVideoCodec;
        set => SetProperty(ref _selectedVideoCodec, value);
    }

    public VideoCodecPresets CodecPreset
    {
        get => _codecPreset;
        set => SetProperty(ref _codecPreset, value);
    }

    /// <summary>
    /// Hardware acceleration mode.
    /// https://trac.ffmpeg.org/wiki/HWAccelIntro
    /// </summary>
    public HardwareAccelerationModes HardwareAcceleration
    {
        get => _hardwareAcceleration;
        set
        {
            SetProperty(ref _hardwareAcceleration, value);

            OnPropertyChanged(nameof(VideoCodecs));

            if (FilteredVideoCodecs.All(c => c.Type != VideoCodec))
                VideoCodec = FilteredVideoCodecs.FirstOrDefault()?.Type ?? VideoCodecs.NotSelected;
        }
    }

    public VideoPixelFormats PixelFormat
    {
        get => _pixelFormat;
        set => SetProperty(ref _pixelFormat, value);
    }

    public Vsyncs Vsync
    {
        get => _vsync;
        set => SetProperty(ref _vsync, value);
    }

    public Framerates Framerate
    {
        get => _framerate;
        set
        {
            SetProperty(ref _framerate, value);

            OnPropertyChanged(nameof(CustomFramerateVisibility));
        }
    }

    public decimal CustomFramerate
    {
        get => _customFramerate;
        set => SetProperty(ref _customFramerate, value);
    }

    public Visibility CustomFramerateVisibility => Framerate == Framerates.Custom ? Visibility.Visible : Visibility.Collapsed;

    public int Pass
    {
        get => _pass;
        set => SetProperty(ref _pass, value);
    }

    public int? ConstantRateFactor
    {
        get => _constantRateFactor;
        set => SetProperty(ref _constantRateFactor, value);
    }

    public Visibility ConstantRateFactorVisibility => SelectedVideoCodec != null && SelectedVideoCodec.CanSetCrf ? Visibility.Visible : Visibility.Collapsed;

    public bool IsVariableBitRate
    {
        get => _isVariableBitRate;
        set
        {
            SetProperty(ref _isVariableBitRate, value);

            OnPropertyChanged(nameof(BitRateVisibility));
            OnPropertyChanged(nameof(QualityLevelVisibility));
        }
    }

    public decimal BitRate
    {
        get => _bitRate;
        set => SetProperty(ref _bitRate, value);
    }

    public Visibility BitRateVisibility => IsVariableBitRate ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// Quality level (-q:v, -qscale:v), in use when having the bitrate mode set to variable.
    /// </summary>
    public int QualityLevel
    {
        get => _qualityLevel;
        set => SetProperty(ref _qualityLevel, value);
    }

    public Visibility QualityLevelVisibility => IsVariableBitRate ? Visibility.Visible : Visibility.Collapsed;

    public RateUnits BitRateUnit
    {
        get => _bitRateUnit;
        set => SetProperty(ref _bitRateUnit, value);
    }

    public decimal MinimumBitRate
    {
        get => _minimumBitRate;
        set => SetProperty(ref _minimumBitRate, value);
    }

    public RateUnits MinimumBitRateUnit
    {
        get => _minimumBitRateUnit;
        set => SetProperty(ref _minimumBitRateUnit, value);
    }

    public decimal MaximumBitRate
    {
        get => _maximumBitRate;
        set => SetProperty(ref _maximumBitRate, value);
    }

    public RateUnits MaximumBitRateUnit
    {
        get => _maximumBitRateUnit;
        set => SetProperty(ref _maximumBitRateUnit, value);
    }

    public decimal RateControlBuffer
    {
        get => _rateControlBuffer;
        set => SetProperty(ref _rateControlBuffer, value);
    }

    public RateUnits RateControlBufferUnit
    {
        get => _rateControlBufferUnit;
        set => SetProperty(ref _rateControlBufferUnit, value);
    }

    public bool IsAncientContainer => Type == ExportFormats.Avi;

    protected VideoPresetViewModel()
    {
        OutputFilenameKey = "S.Preset.Filename.Video";
    }

    public static VideoPresetViewModel FromModel(VideoPreset preset, IPreviewerViewModel exporterViewModel)
    {
        return new()
        {
            Title = preset.Title,
            TitleKey = preset.TitleKey,
            Description = preset.Description,
            DescriptionKey = preset.DescriptionKey,
            IsSelected = preset.IsSelected,
            IsSelectedForEncoder = preset.IsSelectedForEncoder,
            IsDefault = preset.IsDefault,
            HasAutoSave = preset.HasAutoSave,
            CreationDate = preset.CreationDate,
            PickLocation = preset.PickLocation,
            OverwriteMode = preset.OverwriteMode,
            ExportAsProjectToo = preset.ExportAsProjectToo,
            UploadFile = preset.UploadFile,
            UploadService = preset.UploadService,
            SaveToClipboard = preset.SaveToClipboard,
            CopyType = preset.CopyType,
            ExecuteCustomCommands = preset.ExecuteCustomCommands,
            CustomCommands = preset.CustomCommands,
            OutputFolder = preset.OutputFolder,
            OutputFilename = preset.OutputFilename,
            OutputFilenameKey = preset.OutputFilenameKey,
            Extension = preset.Extension,
            PreviewerViewModel = exporterViewModel,
            SettingsMode = preset.SettingsMode,
            Parameters = preset.Parameters,
            VideoCodec = preset.VideoCodec,
            CodecPreset = preset.CodecPreset,
            HardwareAcceleration = preset.HardwareAcceleration,
            Pass = preset.Pass,
            IsVariableBitRate = preset.IsVariableBitRate,
            ConstantRateFactor = preset.ConstantRateFactor,
            BitRate = preset.BitRate,
            QualityLevel = preset.QualityLevel,
            BitRateUnit = preset.BitRateUnit,
            MinimumBitRate = preset.MinimumBitRate,
            MinimumBitRateUnit = preset.MinimumBitRateUnit,
            MaximumBitRate = preset.MaximumBitRate,
            MaximumBitRateUnit = preset.MaximumBitRateUnit,
            RateControlBuffer = preset.RateControlBuffer,
            RateControlBufferUnit = preset.RateControlBufferUnit,
            PixelFormat = preset.PixelFormat,
            Framerate = preset.Framerate,
            CustomFramerate = preset.CustomFramerate,
            Vsync = preset.Vsync,
        };
    }

    public override ExportPreset ToModel()
    {
        return null;
    }

    public override void Reset() { }

    private void FilterVideoCodecs()
    {
        switch (Type)
        {
            case ExportFormats.Avi:
            {
                FilteredVideoCodecs = new ObservableCollection<VideoCodecViewModel>
                {
                    new Mpeg2(),
                    new Mpeg4()
                };
                return;
            }

            case ExportFormats.Mkv:
            {
                if (HardwareAcceleration == HardwareAccelerationModes.On)
                {
                    FilteredVideoCodecs = new ObservableCollection<VideoCodecViewModel>
                    {
                        new X264(),
                        new H264Amf(),
                        new H264Nvenc(),
                        new H264Qsv(),
                        new X265(),
                        new HevcAmf(),
                        new HevcNvenc(),
                        new HevcQsv(),
                        new Vp8(),
                        new Vp9(),
                        new LibAom(),
                        new SvtAv1(),
                        new Rav1E()
                    };
                    return;
                }


                FilteredVideoCodecs = new ObservableCollection<VideoCodecViewModel>
                {
                    new X264(),
                    new X265(),
                    new Vp8(),
                    new Vp9(),
                    new LibAom(),
                    new SvtAv1(),
                    new Rav1E()
                };
                return;
            }

            case ExportFormats.Mov:
            case ExportFormats.Mp4:
            {
                if (HardwareAcceleration == HardwareAccelerationModes.On)
                {
                    FilteredVideoCodecs = new ObservableCollection<VideoCodecViewModel>
                    {
                        new X264(),
                        new H264Amf(),
                        new H264Nvenc(),
                        new H264Qsv(),
                        new X265(),
                        new HevcAmf(),
                        new HevcNvenc(),
                        new HevcQsv()
                    };
                    return;
                }

                FilteredVideoCodecs = new ObservableCollection<VideoCodecViewModel>
                {
                    new X264(),
                    new X265()
                };
                return;
            }

            case ExportFormats.Webm:
            {
                FilteredVideoCodecs = new ObservableCollection<VideoCodecViewModel>
                {
                    new Vp8(),
                    new Vp9(),
                    new LibAom(),
                    new SvtAv1(),
                    new Rav1E()
                };
                return;
            }

            default:
            {
                FilteredVideoCodecs = new ObservableCollection<VideoCodecViewModel>();
                return;
            }
        }
    }
}