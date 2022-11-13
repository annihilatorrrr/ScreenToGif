using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Preset.Export.Image;

namespace ScreenToGif.ViewModel.Presets.Export.Image;

public class ImagePresetViewModel : ExportPresetViewModel
{
    private bool _zipFiles;

    //TODO: Check if this makes sense.
    public bool ZipFiles
    {
        get => _zipFiles;
        set
        {
            SetProperty(ref _zipFiles, value);
            
            Extension = value ? ".zip" : DefaultExtension;
        }
    }

    /// <summary>
    /// Internal accessor for controlling the switch of the ZipFiles property without altering the extension. 
    /// </summary>
    public bool ZipFilesInternal
    {
        get => _zipFiles;
        set
        {
            SetProperty(ref _zipFiles, value);

            OnPropertyChanged(nameof(ZipFiles));
        }
    }
    
    public ImagePresetViewModel()
    {
        OutputFilenameKey = "S.Preset.Filename.Image";
    }

    public static ImagePresetViewModel FromModel(ImagePreset preset, IPreviewerViewModel exporterViewModel)
    {
        switch (preset)
        {
            case BmpPreset bmp:
                return BmpPresetViewModel.FromModel(bmp, exporterViewModel);

            case JpegPreset jpg:
                return JpegPresetViewModel.FromModel(jpg, exporterViewModel);

            case PngPreset png:
                return PngPresetViewModel.FromModel(png, exporterViewModel);

            default:
                return null;
        }
    }
}