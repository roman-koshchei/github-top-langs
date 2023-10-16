using GitHubTopLangs.Lib;
using System.Text;

namespace GitHubTopLangs.Services;

public static class Svg
{
    private static readonly int margin = 30;
    private static readonly int width = 350;

    //width - margin * 2;
    private static readonly int length = 290;

    // circle radius
    private static readonly int cr = 5;

    private static readonly string style;

    static Svg()
    {
        style = $@"
                <style>
                    text {{
                        fill: white;
                        font-size: 12px;
                        font-family: Arial;
                        transform: translate({cr + 10}px, {cr}px);
                    }}
                    circle {{
                        r: {cr}px;
                    }}
                    line {{
                        stroke-width: 10px;
                    }}
                    .bar {{
                        transform: translate(0, 30px);
                    }}
                    .persents {{
                        transform: translate(0, 60px);
                    }}
                </style>
            ".Compact();
    }

    public static string FirstAskCard(string background)
    {
        int height = 100;

        return $@"
        <svg width='{width}' height='{height}' xmlns='http://www.w3.org/2000/svg'>
          {style}
          <rect width='{width}' height='{height}' fill='{background}' />
          <g class='bar'>
            <text>
              Actions should be meaningful!
            </text>
          </g>
          <g class='persents'>
            <text>
              Your request will be chached and showed at next request.
            </text>
          </g>
        </svg>
      ";
    }

    public static string LangCard(IEnumerable<Lang> langs, string background)
    {
        double height = 70 + Math.Ceiling(langs.Count() / 2.0) * 20;

        return $@"
                <svg width='{width}' height='{height}' xmlns='http://www.w3.org/2000/svg'>
                    {style}
                    <rect width='{width}' height='{height}' fill='{background}' />
                    <g class='bar'>
                        {LangBar(langs)}
                    </g>
                    <g class='persents'>
                        {LangPercents(langs)}
                    </g>
                </svg>
            ".Compact();
    }

    private static string LangBar(IEnumerable<Lang> langs)
    {
        StringBuilder sb = new();
        double start = margin;
        double end;

        foreach (Lang lang in langs)
        {
            end = length * lang.Percent + start;
            sb.Append(LangLine(lang, start, end));
            start = end;
        }
        return sb.ToString();
    }

    private static string LangPercents(IEnumerable<Lang> langs)
    {
        StringBuilder builder = new();
        int count = langs.Count();
        string color;
        int y = 0;

        for (int i = 0; i < count; ++i)
        {
            if (!Lang.Colors.TryGetValue(langs.ElementAt(i).Name, out color))
            {
                color = "#fff";
            }

            if (i % 2 == 0)
            {
                builder.Append(LangText(langs.ElementAt(i), 30 + cr, y, color));
            }
            else
            {
                builder.Append(LangText(langs.ElementAt(i), 175 + cr, y, color));
                y += 20;
            }
        }
        return builder.ToString();
    }

    private static string LangText(Lang lang, int x, int y, string color)
    {
        return $@"
                <g transform='translate({x}, {y})'>
                    <circle fill='{color}'/>
                    <text>{lang.Name} {lang.Percent.ToString("P")}</text>
                </g>
            ".Compact();
    }

    private static string LangLine(Lang lang, double start, double end)
    {
        string color;
        if (!Lang.Colors.TryGetValue(lang.Name, out color)) color = "#fff";

        return $"<line x1='{start}' x2='{end}' stroke='{color}' />";
    }
}