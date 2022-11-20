using System.Collections;
using System.Windows;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Events;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Upload;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.Util;

namespace ScreenToGif.ViewModel.Presets.Upload;

//TODO: Properly make the vm responsive
public class UploadPresetViewModel : BaseViewModel, IUploadPreset
{
    private UploadDestinations _type = UploadDestinations.NotDefined;
    private bool _isEnabled = true;
    private string _title = "";
    private string _description = "";
    private bool _isAnonymous;
    private ArrayList _history = new();
    private List<ExportFormats> _allowedTypes;

    private readonly long? _sizeLimit;
    private readonly TimeSpan? _durationLimit;
    private readonly Size? _resolutionLimit;

    public UploadPresetViewModel()
    { }

    //Needs?
    public UploadPresetViewModel(long? sizeLimit, TimeSpan? durationLimit = null, Size? resolutionLimit = null)
    {
        _sizeLimit = sizeLimit;
        _durationLimit = durationLimit;
        _resolutionLimit = resolutionLimit;
    }

    public UploadDestinations Type
    {
        get => _type;
        set
        {
            SetProperty(ref _type, value);

            OnPropertyChanged(nameof(TypeName));
            OnPropertyChanged(nameof(Symbol));
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public FluentSymbols Symbol => Type switch
    {
        UploadDestinations.NotDefined => FluentSymbols.Info,
        UploadDestinations.Custom => FluentSymbols.ArrowSync,
        _ => FluentSymbols.ArrowSyncCircle,
    };

    public bool IsAnonymous
    {
        get => _isAnonymous;
        set
        {
            SetProperty(ref _isAnonymous, value);

            OnPropertyChanged(nameof(Mode));
        }
    }

    public ArrayList History
    {
        get => _history;
        set => SetProperty(ref _history, value);
    }

    public List<ExportFormats> AllowedTypes
    {
        get => _allowedTypes;
        set => SetProperty(ref _allowedTypes, value);
    }

    public string TypeName => Type switch
    {
        UploadDestinations.Imgur => "Imgur",
        UploadDestinations.Gfycat => "Gfycat",
        UploadDestinations.Yandex => "Yandex",
        UploadDestinations.Custom => LocalizationHelper.Get("S.Options.Upload.Preset.Custom"),
        _ => LocalizationHelper.Get("S.Options.Upload.Preset.Select") //Needs?
    };

    public bool HasLimit => HasSizeLimit || HasDurationLimit || HasResolutionLimit;

    public bool HasSizeLimit => _sizeLimit != null;

    public bool HasDurationLimit => _durationLimit != null;

    public bool HasResolutionLimit => _resolutionLimit != null;

    public long? SizeLimit => _sizeLimit;

    public TimeSpan? DurationLimit => _durationLimit;

    public Size? ResolutionLimit => _resolutionLimit;

    public string Limit => (HasLimit ? "▼ " : "") + (HasSizeLimit ? Humanizer.BytesToString(SizeLimit ?? 0L) : "") + (HasSizeLimit && (HasDurationLimit || HasResolutionLimit) ? " • " : "") +
                           (HasDurationLimit ? $"{DurationLimit:mm\':\'ss} m" : "") + (HasDurationLimit && HasResolutionLimit ? " • " : "") + (HasResolutionLimit ? $"{ResolutionLimit?.Width}x{ResolutionLimit?.Height}" : "");

    public string Mode => IsAnonymous ? LocalizationHelper.Get("S.Options.Upload.Preset.Mode.Anonymous") : LocalizationHelper.Get("S.Options.Upload.Preset.Mode.Authenticated");

    public static UploadPresetViewModel FromModel(UploadPreset preset, IPreviewerViewModel exporterViewModel)
    {
        switch (preset)
        {
            //case AnimatedImagePreset image:
            //    return AnimatedImagePresetViewModel.FromModel(image, exporterViewModel);

            //case VideoPreset video:
            //    return VideoPresetViewModel.FromModel(video, exporterViewModel);

            //case ImagePreset image:
            //    return ImagePresetViewModel.FromModel(image, exporterViewModel);

            //case StgPreset stg:
            //    return StgPresetViewModel.FromModel(stg, exporterViewModel);

            //case PsdPreset psd:
            //    return PsdPresetViewModel.FromModel(psd, exporterViewModel);
        }

        return null;
    }

    //Needs?
    public virtual Task<ValidatedEventArgs> IsValid()
    {
        return Task.FromResult((ValidatedEventArgs)null);
    }
}