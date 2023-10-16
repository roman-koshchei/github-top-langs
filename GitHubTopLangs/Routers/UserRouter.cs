using GitHubTopLangs.Lib;
using GitHubTopLangs.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace GitHubTopLangs.Routers;

public static class UserRouter
{
    private record LangCard(string Username, string Svg);
    private static readonly ConcurrentBag<LangCard> db = new();

    public static async Task<IResult> Get(
        [FromServices] BackgroundQueue backgroundQuery,
        [FromQuery] string name,
        [FromQuery] string? background,
        [FromQuery] string[]? exclude,
        [FromQuery] string[]? hide,
        [FromQuery] bool includePrivate = true,
        [FromQuery] bool includeOrgs = true,
        [FromQuery] bool includeForks = true,
        [FromQuery] int count = 5
    )
    {
        var svg = db.FirstOrDefault(x => x.Username == name);

        await backgroundQuery.Enqueue(async services =>
        {
            var scope = services.CreateScope();
            var github = scope.ServiceProvider.GetRequiredService<Github>();
            var langs = await github.CountUserLangs(
                name, exclude, hide ?? Enumerable.Empty<string>(),
                includePrivate, includeOrgs, includeForks, count
            );

            var svgCard = Svg.LangCard(langs, background ?? "#3c4043");
            db.Add(new LangCard(name, svgCard));
        });

        if (svg == null) return Results.Accepted();
        return Results.Content(svg.Svg, "image/svg+xml");
    }
}