using GitHubExplorer.Api.Data;
using GitHubExplorer.Api.GitHub;
using GitHubExplorer.Api.Infrastructure;
using GitHubExplorer.Api.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connString = builder.Configuration.GetConnectionString("GitHubExplorerDb")
    ?? throw new InvalidOperationException("Connection string 'GitHubExplorerDb' non configurata.");

builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection("ApiKey"));

builder.Services.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyDefaults.AuthenticationScheme, null);
builder.Services.AddAuthorization();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection("GitHub"));
builder.Services.AddHttpClient<IGitHubClient, GitHubClient>((sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<GitHubOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
    http.DefaultRequestHeaders.UserAgent.ParseAdd(opt.UserAgent);
    http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    if (!string.IsNullOrWhiteSpace(opt.Token))
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opt.Token);
});

builder.Services.AddSingleton(new MySqlConnectionFactory(connString));
builder.Services.AddScoped<IFavoritesRepository, FavoritesRepository>();


var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
