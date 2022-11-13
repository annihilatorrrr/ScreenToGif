using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Webp;

namespace ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Webp;

public class WebpPresetViewModel : AnimatedImagePresetViewModel
{
    public WebpPresetViewModel()
    {
        Type = ExportFormats.Webp;
        Extension = ".webp";
    }

    public static WebpPresetViewModel FromModel(WebpPreset preset, IPreviewerViewModel previewerViewModel)
    {
        switch (preset)
        {
            case FfmpegWebpPreset ffmpeg:
                return FfmpegWebpPresetViewModel.FromModel(ffmpeg, previewerViewModel);

            default:
                return null;
        }
    }
}