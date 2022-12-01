using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.ViewModels;
using ScreenToGif.ViewModel.Presets.Export;
using ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Apng;
using ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Gif;
using ScreenToGif.ViewModel.Presets.Export.AnimatedImage.Webp;
using ScreenToGif.ViewModel.Presets.Export.Image;
using ScreenToGif.ViewModel.Presets.Export.Other;
using ScreenToGif.ViewModel.Presets.Export.Video.Avi;
using ScreenToGif.ViewModel.Presets.Export.Video.Mkv;
using ScreenToGif.ViewModel.Presets.Export.Video.Mov;
using ScreenToGif.ViewModel.Presets.Export.Video.Mp4;
using ScreenToGif.ViewModel.Presets.Export.Video.Webm;

namespace ScreenToGif.ViewModel.Dialogs;

public class AddPresetViewModel : BaseViewModel
{
    private ExportFormats _format;
    private string _title;
    private string _description;
    private EncoderTypes _encoder;
    private List<EncoderTypes> _availableEncoders;

    public ExportFormats Format
    {
        get => _format;
        set => SetProperty(ref _format, value);
    }

    public string Title
    {
        get => _title;
        set
        {
            SetProperty(ref _title, value);

            OnPropertyChanged(nameof(IsValid));
        }
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public EncoderTypes Encoder
    {
        get => _encoder;
        set => SetProperty(ref _encoder, value);
    }

    public List<EncoderTypes> AvailableEncoders
    {
        get => _availableEncoders;
        set => SetProperty(ref _availableEncoders, value);
    }

    public ExportPresetViewModel AssembledPreset
    {
        get
        {
            switch (Encoder, Format)
            {
                //ScreenToGif
                case (EncoderTypes.ScreenToGif, ExportFormats.Gif):
                    return new EmbeddedGifPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                case (EncoderTypes.ScreenToGif, ExportFormats.Apng):
                    return new EmbeddedApngPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                case (EncoderTypes.ScreenToGif, ExportFormats.Jpeg):
                    return new JpegPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                case (EncoderTypes.ScreenToGif, ExportFormats.Png):
                    return new PngPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                case (EncoderTypes.ScreenToGif, ExportFormats.Bmp):
                    return new BmpPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                case (EncoderTypes.ScreenToGif, ExportFormats.Psd):
                    return new PsdPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                case (EncoderTypes.ScreenToGif, ExportFormats.Stg):
                    return new StgPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                //Gifski
                case (EncoderTypes.Gifski, ExportFormats.Gif):
                    return new GifskiGifPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                //KGySoft
                case (EncoderTypes.KGySoft, ExportFormats.Gif):
                    return new KGySoftGifPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                //FFmpeg
                case (EncoderTypes.FFmpeg, ExportFormats.Gif):
                    return new FfmpegGifPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                case (EncoderTypes.FFmpeg, ExportFormats.Apng):
                    return new FfmpegApngPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                case (EncoderTypes.FFmpeg, ExportFormats.Webp):
                    return new FfmpegWebpPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                case (EncoderTypes.FFmpeg, ExportFormats.Avi):
                    return new FfmpegAviPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                case (EncoderTypes.FFmpeg, ExportFormats.Mkv):
                    return new FfmpegMkvPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                case (EncoderTypes.FFmpeg, ExportFormats.Mov):
                    return new FfmpegMovPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                case (EncoderTypes.FFmpeg, ExportFormats.Mp4):
                    return new FfmpegMp4PresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                case (EncoderTypes.FFmpeg, ExportFormats.Webm):
                    return new FfmpegWebmPresetViewModel
                    {
                        Title = Title,
                        Description = Description
                    };

                default:
                    throw new Exception("Invalid encoder and file type.");
            }
        }
    }

    public bool IsValid => Title is { Length: > 0 };
}