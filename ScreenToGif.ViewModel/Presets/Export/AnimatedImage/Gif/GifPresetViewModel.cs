using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;

namespace ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Gif;

public abstract class GifPresetViewModel : AnimatedImagePresetViewModel
{
    private bool _useGlobalColorTable;
    
    public bool UseGlobalColorTable
    {
        get => _useGlobalColorTable;
        set => SetProperty(ref _useGlobalColorTable, value);
    }

    protected GifPresetViewModel()
    {
        Type = ExportFormats.Gif;
        Extension = ".gif";
    }

    public static GifPresetViewModel FromModel(GifPreset preset, IPreviewerViewModel previewerViewModel)
    {
        switch (preset)
        {
            case EmbeddedGifPreset embedded:
                return EmbeddedGifPresetViewModel.FromModel(embedded, previewerViewModel);

            case FfmpegGifPreset ffmpeg:
                return FfmpegGifPresetViewModel.FromModel(ffmpeg, previewerViewModel);

            case GifskiGifPreset gifski:
                return GifskiGifPresetViewModel.FromModel(gifski, previewerViewModel);

            case KGySoftGifPreset kgy:
                return KGySoftGifPresetViewModel.FromModel(kgy, previewerViewModel);

            case SystemGifPreset system:
                return SystemGifPresetViewModel.FromModel(system, previewerViewModel);

            default:
                return null;
        }
    }
}