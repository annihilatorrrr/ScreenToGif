using ScreenToGif.Domain.Enums;

namespace ScreenToGif.ViewModel.Presets.Export.Image;

public class JpegPresetViewModel : ImagePresetViewModel
{
    public JpegPresetViewModel()
    {
        Type = ExportFormats.Jpeg;
        Extension = ".jpeg";
    }
    
    public static JpegPresetViewModel Default => new()
    {
        TitleKey = "S.Preset.Default.Title",
        DescriptionKey = "S.Preset.Default.Description",
        HasAutoSave = true,
        IsSelectedForEncoder = true,
        IsDefault = true,
        CreationDate = new DateTime(2021, 02, 20)
    };
}