using System.Windows.Media;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;
using ScreenToGif.Util;
using System.Windows;

namespace ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Gif;

/// <summary>
/// Settings for the a Gif preset with the built-in (embedded) encoder.
/// </summary>
public class EmbeddedGifPresetViewModel : GifPresetViewModel
{
    private bool _useGlobalColorTable;
    private ColorQuantizationTypes _quantizer = ColorQuantizationTypes.Neural;
    private int _samplingFactor = 1;
    private int _maximumColorCount = 256;
    private bool _enableTransparency;
    private bool _selectTransparencyColor;
    private Color _transparencyColor = Colors.Black;
    private bool _detectUnchanged = true;
    private bool _paintTransparent = true;
    private Color _chromaKey = Color.FromRgb(50, 205, 50);

    public bool UseGlobalColorTable
    {
        get => _useGlobalColorTable;
        set => SetProperty(ref _useGlobalColorTable, value);
    }

    public ColorQuantizationTypes Quantizer
    {
        get => _quantizer;
        set
        {
            SetProperty(ref _quantizer, value);

            OnPropertyChanged(nameof(SamplingVisibility));
            OnPropertyChanged(nameof(GlobalColorVisibility));
        }
    }

    public Visibility SamplingVisibility => Quantizer == ColorQuantizationTypes.Neural ? Visibility.Visible : Visibility.Collapsed;

    public Visibility GlobalColorVisibility => Quantizer is ColorQuantizationTypes.Neural or ColorQuantizationTypes.MedianCut or ColorQuantizationTypes.MostUsed ? Visibility.Visible : Visibility.Collapsed;

    public int SamplingFactor
    {
        get => _samplingFactor;
        set => SetProperty(ref _samplingFactor, value);
    }

    public int MaximumColorCount
    {
        get => _maximumColorCount;
        set => SetProperty(ref _maximumColorCount, value);
    }

    public bool EnableTransparency
    {
        get => _enableTransparency;
        set
        {
            SetProperty(ref _enableTransparency, value);

            if (_enableTransparency)
                DetectUnchanged = false;

            OnPropertyChanged(nameof(SelectTransparencyColorVisibility));
            OnPropertyChanged(nameof(DetectUnchangedEnabled));
        }
    }

    public Visibility SelectTransparencyColorVisibility => EnableTransparency ? Visibility.Visible : Visibility.Collapsed;
    
    public bool SelectTransparencyColor
    {
        get => _selectTransparencyColor;
        set
        {
            SetProperty(ref _selectTransparencyColor, value);

            OnPropertyChanged(nameof(TransparencyColorVisibility));
        }
    }

    public Visibility TransparencyColorVisibility => SelectTransparencyColor ? Visibility.Visible : Visibility.Collapsed;

    public Color TransparencyColor
    {
        get => _transparencyColor;
        set => SetProperty(ref _transparencyColor, value);
    }

    public bool DetectUnchanged
    {
        get => _detectUnchanged;
        set
        {
            SetProperty(ref _detectUnchanged, value);

            OnPropertyChanged(nameof(PaintTransparentVisibility));
            OnPropertyChanged(nameof(ChromaKeyVisibility));
        }
    }

    public bool DetectUnchangedEnabled => !EnableTransparency;

    public Visibility PaintTransparentVisibility => DetectUnchanged ? Visibility.Visible : Visibility.Collapsed;

    public bool PaintTransparent
    {
        get => _paintTransparent;
        set
        {
            SetProperty(ref _paintTransparent, value);

            OnPropertyChanged(nameof(ChromaKeyVisibility));
        }
    }

    public Visibility ChromaKeyVisibility => DetectUnchanged && PaintTransparent ? Visibility.Visible : Visibility.Collapsed;

    public Color ChromaKey
    {
        get => _chromaKey;
        set => SetProperty(ref _chromaKey, value);
    }
    
    public EmbeddedGifPresetViewModel()
    {
        Encoder = EncoderTypes.ScreenToGif;
    }

    public static List<EmbeddedGifPresetViewModel> Defaults => new()
    {
        new EmbeddedGifPresetViewModel
        {
            TitleKey = "S.Preset.Default.Title",
            DescriptionKey = "S.Preset.Default.Description",
            HasAutoSave = true,
            IsSelected = true,
            IsDefault = true,
            IsSelectedForEncoder = true,
            CreationDate = new DateTime(2021, 02, 20),

            Quantizer = ColorQuantizationTypes.Neural,
            SamplingFactor = 10
        },

        new EmbeddedGifPresetViewModel
        {
            TitleKey = "S.Preset.Gif.Embedded.High.Title",
            DescriptionKey = "S.Preset.Gif.Embedded.High.Description",
            HasAutoSave = true,
            IsDefault = true,
            IsSelectedForEncoder = true,
            CreationDate = new DateTime(2021, 02, 20),

            Quantizer = ColorQuantizationTypes.Neural
        },

        new EmbeddedGifPresetViewModel
        {
            TitleKey = "S.Preset.Gif.Embedded.Transparent.Title",
            DescriptionKey = "S.Preset.Gif.Embedded.Transparent.Description",
            HasAutoSave = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20),

            Quantizer = ColorQuantizationTypes.Neural,
            EnableTransparency = true,
            DetectUnchanged = false,
            PaintTransparent = false,
        },

        new EmbeddedGifPresetViewModel
        {
            TitleKey = "S.Preset.Gif.Embedded.Graphics.Title",
            DescriptionKey = "S.Preset.Gif.Embedded.Graphics.Description",
            HasAutoSave = true,
            IsDefault = true,
            CreationDate = new DateTime(2021, 02, 20),

            Quantizer = ColorQuantizationTypes.Octree
        }
    };

    public static EmbeddedGifPresetViewModel FromModel(EmbeddedGifPreset preset, IPreviewerViewModel exporterViewModel)
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
            Looped = preset.Looped,
            RepeatForever = preset.RepeatForever,
            RepeatCount = preset.RepeatCount,
            UseGlobalColorTable = preset.UseGlobalColorTable,
            Quantizer = preset.Quantizer,
            SamplingFactor = preset.SamplingFactor,
            MaximumColorCount = preset.MaximumColorCount,
            EnableTransparency = preset.EnableTransparency,
            SelectTransparencyColor = preset.SelectTransparencyColor,
            TransparencyColor = preset.TransparencyColor,
            DetectUnchanged = preset.DetectUnchanged,
            PaintTransparent = preset.PaintTransparent,
            ChromaKey = preset.ChromaKey
        };
    }

    public override ExportPreset ToModel()
    {
        return new EmbeddedGifPreset
        {
            Title = Title,
            TitleKey = TitleKey,
            Description = Description,
            DescriptionKey = DescriptionKey,
            IsSelected = IsSelected,
            IsSelectedForEncoder = IsSelectedForEncoder,
            IsDefault = IsDefault,
            HasAutoSave = HasAutoSave,
            CreationDate = CreationDate,
            PickLocation = PickLocation,
            OverwriteMode = OverwriteMode,
            ExportAsProjectToo = ExportAsProjectToo,
            UploadFile = UploadFile,
            UploadService = UploadService,
            SaveToClipboard = SaveToClipboard,
            CopyType = CopyType,
            ExecuteCustomCommands = ExecuteCustomCommands,
            CustomCommands = CustomCommands,
            OutputFolder = OutputFolder,
            OutputFilename = OutputFilename,
            OutputFilenameKey = OutputFilenameKey,
            Extension = Extension,
            Looped = Looped,
            RepeatForever = RepeatForever,
            RepeatCount = RepeatCount,
            UseGlobalColorTable = UseGlobalColorTable,
            Quantizer = Quantizer,
            SamplingFactor = SamplingFactor,
            MaximumColorCount = MaximumColorCount,
            EnableTransparency = EnableTransparency,
            SelectTransparencyColor = SelectTransparencyColor,
            TransparencyColor = TransparencyColor,
            DetectUnchanged = DetectUnchanged,
            PaintTransparent = PaintTransparent,
            ChromaKey = ChromaKey
        };
    }

    public override void Reset()
    {
        var preset = Defaults.First(f => f.TitleKey == TitleKey);

        Title = LocalizationHelper.Get(preset.TitleKey).Replace("{0}", preset.DefaultExtension);
        Description = LocalizationHelper.Get(preset.DescriptionKey);
        IsSelected = preset.IsSelected;
        IsSelectedForEncoder = preset.IsSelectedForEncoder;
        IsDefault = preset.IsDefault;
        HasAutoSave = preset.HasAutoSave;
        CreationDate = preset.CreationDate;
        PickLocation = preset.PickLocation;
        OverwriteMode = preset.OverwriteMode;
        ExportAsProjectToo = preset.ExportAsProjectToo;
        UploadFile = preset.UploadFile;
        UploadService = preset.UploadService;
        SaveToClipboard = preset.SaveToClipboard;
        CopyType = preset.CopyType;
        ExecuteCustomCommands = preset.ExecuteCustomCommands;
        CustomCommands = preset.CustomCommands;
        OutputFolder = preset.OutputFolder;
        OutputFilename = preset.OutputFilename;
        OutputFilenameKey = preset.OutputFilenameKey;
        Extension = preset.Extension;
        Looped = preset.Looped;
        RepeatForever = preset.RepeatForever;
        RepeatCount = preset.RepeatCount;
        UseGlobalColorTable = preset.UseGlobalColorTable;
        Quantizer = preset.Quantizer;
        SamplingFactor = preset.SamplingFactor;
        MaximumColorCount = preset.MaximumColorCount;
        EnableTransparency = preset.EnableTransparency;
        SelectTransparencyColor = preset.SelectTransparencyColor;
        TransparencyColor = preset.TransparencyColor;
        DetectUnchanged = preset.DetectUnchanged;
        PaintTransparent = preset.PaintTransparent;
        ChromaKey = preset.ChromaKey;
    }
}