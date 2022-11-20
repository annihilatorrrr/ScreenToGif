using ScreenToGif.Domain.Enums;
using System.Collections;
using System.Runtime.Serialization;

namespace ScreenToGif.Domain.Models.Preset.Upload;

public class UploadPreset
{
    public UploadDestinations Type { get; set; }

    public bool IsEnabled { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string Title { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string Description { get; set; }

    public bool IsAnonymous { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public ArrayList History { get; set; }
}