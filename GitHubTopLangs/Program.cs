using GitHubTopLangs.Lib;
using GitHubTopLangs.Routers;
using GitHubTopLangs.Services;

Env.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin());
});

// Add services to the container.
builder.Services.AddSingleton<BackgroundQueue>();
builder.Services.AddHostedService<BackgroundQueue.HostedService>();

var githubToken = Env.GetRequired("GITHUB_TOKEN");
builder.Services.AddScoped(_ => new Github(githubToken));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/", UserRouter.Get);

app.Run();