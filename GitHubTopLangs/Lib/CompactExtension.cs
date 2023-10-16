using System.Text;

namespace GitHubTopLangs.Lib;

public static class CompactExtension
{
    /// <summary>
    /// Remove all double white spaces from string.
    /// Mostly used with JSON or HTML string.
    /// </summary>
    public static string Compact(this string source)
    {
        var builder = new StringBuilder(source.Length);
        bool previousWhitespace = false;

        for (int i = 0; i < source.Length; ++i)
        {
            char c = source[i];

            if (char.IsWhiteSpace(c))
            {
                previousWhitespace = true;
                continue;
            }

            if (previousWhitespace)
            {
                builder.Append(' ');
                previousWhitespace = false;
            }

            builder.Append(c);
        }

        return builder.ToString();
    }
}