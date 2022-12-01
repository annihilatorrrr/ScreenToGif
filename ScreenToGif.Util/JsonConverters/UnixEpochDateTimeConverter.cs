using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ScreenToGif.Util.JsonConverters;

public sealed class UnixEpochDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly Regex Regex = new("^/Date\\(([+-]*\\d+)\\)/$", RegexOptions.CultureInvariant);

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var formatted = reader.GetString()!;
        var match = Regex.Match(formatted);

        if (!match.Success || !long.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, out long unixTime))
            throw new JsonException($"Date does not match expected format: '{formatted}'");

        return Epoch.AddMilliseconds(unixTime);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var unixTime = Convert.ToInt64((value - Epoch).TotalMilliseconds);

        var formatted = FormattableString.Invariant($"/Date({unixTime})/");
        writer.WriteStringValue(formatted);
    }
}