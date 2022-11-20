using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Apng;

namespace ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Apng;

public abstract class ApngPresetViewModel : AnimatedImagePresetViewModel
{
    protected ApngPresetViewModel()
    {
        Type = ExportFormats.Apng;
        Extension = ".apng";
    }

    public static ApngPresetViewModel FromModel(ApngPreset preset, IPreviewerViewModel previewerViewModel)
    {
        switch (preset)
        {
            case EmbeddedApngPreset embedded:
                return EmbeddedApngPresetViewModel.FromModel(embedded, previewerViewModel);

            case FfmpegApngPreset ffmpeg:
                return FfmpegApngPresetViewModel.FromModel(ffmpeg, previewerViewModel);

            default:
                return null;
        }
    }
}