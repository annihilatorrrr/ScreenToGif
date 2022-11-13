using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Apng;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;
using ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Webp;
using ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Apng;
using ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Gif;
using ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Webp;

namespace ScreenToGif.ViewModel.Presets.Export.AnimatedImage;

public class AnimatedImagePresetViewModel : ExportPresetViewModel
{
    private bool _looped = true;
    private bool _repeatForever = true;
    private int _repeatCount = 2;
    
    public bool Looped
    {
        get => _looped;
        set => SetProperty(ref _looped, value);
    }

    public bool RepeatForever
    {
        get => _repeatForever;
        set => SetProperty(ref _repeatForever, value);
    }

    public int RepeatCount
    {
        get => _repeatCount;
        set => SetProperty(ref _repeatCount, value);
    }
    
    public AnimatedImagePresetViewModel()
    {
        OutputFilenameKey = "S.Preset.Filename.Animation";
    }

    public static AnimatedImagePresetViewModel FromModel(AnimatedImagePreset preset, IPreviewerViewModel exporterViewModel)
    {
        switch (preset)
        {
            case ApngPreset apng:
                return ApngPresetViewModel.FromModel(apng, exporterViewModel);

            case GifPreset gif:
                return GifPresetViewModel.FromModel(gif, exporterViewModel);

            case WebpPreset webp:
                return WebpPresetViewModel.FromModel(webp, exporterViewModel);

            default:
                return null;
        }
    }
}