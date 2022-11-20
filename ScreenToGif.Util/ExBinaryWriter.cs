using System.IO;
using System.Text;

namespace ScreenToGif.Util;

public class ExBinaryWriter : BinaryWriter
{
    public long Position { get; set; }

    public ExBinaryWriter(Stream stream) : base(stream)
    { }

    public override void Write(byte value)
    {
        Position++;

        base.Write(value);
    }

    public override void Write(bool value)
    {
        Position++;

        base.Write(value);
    }

    public override void Write(short value)
    {
        Position += 2;

        base.Write(value);
    }

    public override void Write(ushort value)
    {
        Position += 2;

        base.Write(value);
    }

    public override void Write(uint value)
    {
        Position += 4;

        base.Write(value);
    }

    public override void Write(int value)
    {
        Position += 4;

        base.Write(value);
    }

    public override void Write(long value)
    {
        Position += 8;

        base.Write(value);
    }

    public override void Write(ulong value)
    {
        Position += 8;

        base.Write(value);
    }

    public override void Write(byte[] buffer)
    {
        Position += buffer.Length;

        base.Write(buffer);
    }

    public void WriteTwice(byte[] buffer)
    {
        Position += buffer.Length + buffer.Length;

        base.Write(buffer);
        base.Write(buffer);
    }

    public void WritePascalString(string value, bool padded = false)
    {
        var bytes = string.IsNullOrEmpty(value) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(value);

        Write((byte)bytes.Length); //String size, 1 byte.
        Write(bytes, 0, bytes.Length); //String, XX bytes.
        
        if (!padded)
            return;

        var padding = 4 - (bytes.Length + 1) % 4;

        if (padding == 4)
            return;

        //There's zero padding if equals to 4.
        for (var i = 0; i < padding; i++)
            Write(0);
    }

    public void WritePascalString(byte[] bytes, bool padded = true)
    {
        Write((byte)bytes.Length); //String size, 1 byte.
        Write(bytes, 0, bytes.Length); //String, XX bytes (Max 31).

        if (!padded)
            return;

        var padding = 4 - (bytes.Length + 1) % 4;

        if (padding == 4)
            return;

        //There's zero padding if equals to 4.
        for (var i = 0; i < padding; i++)
            Write(0);
    }

    public void WritePascalStringUInt16(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Write((ushort)0); //String size, 2 bytes.
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(value);

        Write((ushort)bytes.Length); //String size, 2 bytes.
        Write(bytes, 0, bytes.Length); //String, XX bytes.
    }

    public void WritePascalStringUInt32(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Write((uint)0); //String size, 4 bytes.
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(value);

        Write((uint)bytes.Length); //String size, 4 bytes.
        Write(bytes, 0, bytes.Length); //String, XX bytes.
    }
}