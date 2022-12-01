using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ScreenToGif.Util.JsonConverters;

public sealed class UnixEpochDateTimeOffsetConverter : JsonConverter<DateTime>
{
    private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly Regex Regex = new("^/Date\\(([+-]*\\d+)([+-])(\\d{2})(\\d{2})\\)/$", RegexOptions.CultureInvariant);

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var formatted = reader.GetString()!;
        var match = Regex.Match(formatted);

        if (!match.Success || !long.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixTime)
                           || !int.TryParse(match.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var hours)
                           || !int.TryParse(match.Groups[4].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes))
            throw new JsonException($"Date does not match expected format: '{formatted}'");

        var sign = match.Groups[2].Value[0] == '+' ? 1 : -1;
        var utcOffset = new TimeSpan(hours * sign, minutes * sign, 0);

        return Epoch.AddMilliseconds(unixTime).Add(utcOffset);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var unixTime = Convert.ToInt64((value - Epoch).TotalMilliseconds);
        var utcOffset = value - value.ToUniversalTime();

        var formatted = FormattableString.Invariant($"/Date({unixTime}{(utcOffset >= TimeSpan.Zero ? "+" : "-")}{utcOffset:hhmm})/");
        writer.WriteStringValue(formatted);
    }
}