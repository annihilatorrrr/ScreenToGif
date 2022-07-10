using System.IO;
using System.Text.RegularExpressions;

namespace ScreenToGif.Util;

public static class PathHelper
{
    /// <summary>
    /// Puts the current date/time into filename, replacing the format typed in between two questions marks.
    /// Such as 'Animation ?dd-MM-yy?' -> 'Animation 21-04-21'
    /// Only some of the formats are available, since there's a file name limitation from Windows.
    /// https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings 
    /// </summary>
    /// <param name="name">The name of the file, with the date/time format.</param>
    /// <returns>The name with the date and time.</returns>
    public static string ReplaceRegexInName(string name)
    {
        const string dateTimeFileNameRegEx = @"[?]([ymdhsfzgkt]+[-_ ]*)+[?]";

        if (!Regex.IsMatch(name, dateTimeFileNameRegEx, RegexOptions.IgnoreCase))
            return name;

        var match = Regex.Match(name, dateTimeFileNameRegEx, RegexOptions.IgnoreCase);
        var date = DateTime.Now.ToString(Regex.Replace(match.Value, "[?]", ""));

        return name.Replace(match.ToString(), date);
    }

    /// <summary>
    /// When dealing with relative paths, the app will fails to point to the right folder when starting it via the "Open with..." or automatic startup methods.
    /// </summary>
    public static string AdjustPath(string path)
    {
        //If the path is relative, File.Exists() was returning C:\\Windows\\System32\ffmpeg.exe when the app was launched from the "Open with" context menu.
        //So, in order to get the correct location, I need to combine the current base directory with the relative path.
        if (!string.IsNullOrWhiteSpace(path) && !Path.IsPathRooted(path))
        {
            var adjusted = path.StartsWith("." + Path.AltDirectorySeparatorChar) ? path.TrimStart('.', Path.AltDirectorySeparatorChar) :
                path.StartsWith("." + Path.DirectorySeparatorChar) ? path.TrimStart('.', Path.DirectorySeparatorChar) : path;

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, adjusted);
        }

        return path;
    }
}