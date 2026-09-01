using GitHubExplorer.Web.ApiClients;
using GitHubExplorer.Web.Components;
using GitHubExplorer.Web.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));
builder.Services.AddTransient<ApiKeyHandler>();

builder.Services.AddHttpClient<IGitHubExplorerApiClient, GitHubExplorerApiClient>((sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
})
.AddHttpMessageHandler<ApiKeyHandler>();

builder.Services.AddScoped<ToastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
