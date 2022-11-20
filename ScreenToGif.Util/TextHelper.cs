namespace ScreenToGif.Util;

public static class TextHelper
{
    public static string FindTextBetween(this string text, string left, string right)
    {
        //Left delimiter.
        var beginIndex = text.IndexOf(left, StringComparison.InvariantCulture);

        if (beginIndex == -1)
            return string.Empty;

        beginIndex += left.Length;

        //Right delimiter.
        var endIndex = text.IndexOf(right, beginIndex, StringComparison.InvariantCulture);

        if (endIndex == -1)
            return string.Empty;

        return text.Substring(beginIndex, endIndex - beginIndex).Trim();
    }

    public static DateTime ConvertJsonStringToDateTime(this string jsonTime)
    {
        if (string.IsNullOrEmpty(jsonTime) || jsonTime.IndexOf("Date", StringComparison.InvariantCulture) <= -1)
            return DateTime.MinValue;

        var milis = jsonTime.Substring(jsonTime.IndexOf("(", StringComparison.InvariantCulture) + 1);
        
        var positiveSign = milis.IndexOf("+", StringComparison.InvariantCulture);
        var negativeSign = milis.IndexOf("-", StringComparison.InvariantCulture);
        var sign = positiveSign > 0 ? "+" : negativeSign > 0 ? "-" : null;

        if (sign == null)
        {
            milis = milis.Substring(0, milis.IndexOf(")", StringComparison.InvariantCulture));

            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToInt64(milis));
        }

        var hours = milis.Substring(milis.IndexOf(sign, StringComparison.InvariantCulture));
        milis = milis.Substring(0, milis.IndexOf(sign, StringComparison.InvariantCulture));
        hours = hours.Substring(0, hours.IndexOf(")", StringComparison.InvariantCulture));

        return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToInt64(milis)).AddHours(Convert.ToInt64(hours) / 100d);
    }
}