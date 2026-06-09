using System.Text.RegularExpressions;

public static class StringExtensions
{


    /// <summary>
    /// Inserts a space before every capital letter.
    /// </summary>
    /// <param name="src">A string wouthout spaces.</param>
    /// <param name="canContainAcronyms">Tries to recognize acyronyms to avoid splitting them. This may cause issues when an acronym is either lead or followed by a one-letter word (e.g. I, a).</param>
    /// <returns>A string with spaces.</returns>
    public static string InsertSpaces(this string src, bool canContainAcronyms = false)
    {
        if (canContainAcronyms)
        {
            return Regex.Replace(src, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
        }
        else
        {
            return Regex.Replace(Regex.Replace(src, @"(\p{L})(\p{Lu})", "$1 $2"), @"(\p{L})(\p{Lu})", "$1 $2");
        }
    }

    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

}