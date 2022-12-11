using System.IO;
using System.Text;

namespace ScreenToGif.Util;

internal class ExBinaryReader : BinaryReader
{
    public ExBinaryReader(Stream input) : base(input)
    { }

    public ExBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
    { }

    public ExBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
    { }

    public string ReadPascalString()
    {
        var size = BaseStream.ReadByte();

        if (size <= 0)
            return null;

        return Encoding.UTF8.GetString(BaseStream.ReadBytes(size));
    }

    public string ReadPascalStringUInt16()
    {
        var size = BaseStream.ReadUInt16();

        if (size <= 0)
            return null;

        return Encoding.UTF8.GetString(BaseStream.ReadBytes(size));
    }

    public string ReadPascalStringUInt32()
    {
        var size = BaseStream.ReadUInt32();

        if (size <= 0)
            return null;

        return Encoding.UTF8.GetString(BaseStream.ReadBytes(size));
    }
}