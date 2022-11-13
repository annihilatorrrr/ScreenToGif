using ScreenToGif.Domain.Enums;
using System.Runtime.Serialization;

namespace ScreenToGif.Domain.Models.Preset.Export;

public class ExportPreset
{
    public ExportFormats Type { get; set; }

    public EncoderTypes Encoder { get; set; }

    public string Title { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string TitleKey { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string Description { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string DescriptionKey { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string DefaultExtension { get; set; }

    /// <summary>
    /// True if this preset was the latest selected preset for the selected file type.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// True if this preset was the latest selected preset for the selected file type and encoder.
    /// </summary>
    public bool IsSelectedForEncoder { get; set; }

    /// <summary>
    /// True if this preset was provided by the app.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// True if this preset automatically saves it's new property values when the user changes something.
    /// </summary>
    public bool HasAutoSave { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public DateTime CreationDate { get; set; }


    public bool PickLocation { get; set; }

    public OverwriteModes OverwriteMode { get; set; }

    public bool ExportAsProjectToo { get; set; }

    public bool UploadFile { get; set; }

    public string UploadService { get; set; }

    public bool SaveToClipboard { get; set; }

    public CopyModes CopyType { get; set; }

    public bool ExecuteCustomCommands { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string CustomCommands { get; set; }


    [DataMember(EmitDefaultValue = false)]
    public string OutputFolder { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string OutputFilename { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string OutputFilenameKey { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string Extension { get; set; }
}