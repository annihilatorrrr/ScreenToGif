using ScreenToGif.Domain.Enums;
using System.Windows.Media;

namespace ScreenToGif.Domain.Models.Preset.Export.AnimatedImage.Gif;

public class KGySoftGifPreset : GifPreset
{
    /// <summary>
    /// Gets or sets the quantizer identifier in {TypeName}.{MethodName} format.
    /// </summary>
    public string QuantizerId { get; set; }

    /// <summary>
    /// Gets or sets the background color for alpha pixels that will not be transparent in the result.
    /// </summary>
    public Color BackColor { get; set; }

    /// <summary>
    /// Gets or sets the alpha threshold under which a color is considered transparent.
    /// This property is ignored by quantizers that do not support transparency.
    /// </summary>
    public byte AlphaThreshold { get; set; }

    /// <summary>
    /// Gets or sets the lowest input brightness to consider the result color white.
    /// This property is considered only by the black and white quantizer.
    /// </summary>
    public byte WhiteThreshold { get; set; }

    /// <summary>
    /// Gets or sets whether the palette entries are mapped from the color directly.
    /// This property is ignored by quantizers that do not support direct mapping.
    /// </summary>
    public bool DirectMapping { get; set; }

    /// <summary>
    /// Gets or sets the maximum palette size per frame.
    /// This property is ignored by predefined colors quantizers.
    /// </summary>
    public int PaletteSize { get; set; }

    /// <summary>
    /// Gets or sets the bit level used by an optimized quantizer.
    /// This property is ignored by predefined colors quantizers.
    /// </summary>
    public byte? BitLevel { get; set; }

    /// <summary>
    /// Gets or sets the ditherer identifier in {TypeName}[.{PropertyName}] format.
    /// </summary>
    public string DithererId { get; set; }

    /// <summary>
    /// Gets or sets the strength of the ditherer.
    /// This property is ignored by error diffusion ditherers.
    /// </summary>
    public float Strength { get; set; }

    /// <summary>
    /// Gets or sets the seed of ditherer.
    /// This property is ignored by non-randomized ditherers.
    /// </summary>
    public int? Seed { get; set; }

    /// <summary>
    /// Gets or sets whether the ditherer uses serpentine processing.
    /// This property is used only by error diffusion ditherers.
    /// </summary>
    public bool IsSerpentineProcessing { get; set; }

    /// <summary>
    /// Gets or sets whether the encoder is allowed to save the changed image parts.
    /// </summary>
    public bool AllowDeltaFrames { get; set; }

    /// <summary>
    /// Gets or sets whether the encoder is allowed clip the transparent border of the frames.
    /// </summary>
    public bool AllowClippedFrames { get; set; }

    /// <summary>
    /// If <see cref="AllowDeltaFrames"/> is <see langword="true"/>, then gets or sets the allowed maximum tolerance when detecting changes.
    /// </summary>
    public byte DeltaTolerance { get; set; }

    public KGySoftGifPreset()
    {
        Encoder = EncoderTypes.KGySoft;
    }
}