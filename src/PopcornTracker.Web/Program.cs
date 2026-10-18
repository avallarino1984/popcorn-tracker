using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PopcornTracker.Web;
using PopcornTracker.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// French (Canada) date formatting everywhere, whatever the browser language.
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("fr-CA");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("fr-CA");

// Everything below comes from wwwroot/appsettings.json and is public by design:
// access to the data is enforced by Supabase RLS, not by hiding these values.
var supabase = new SupabaseOptions
{
    // Accept the "Data API" URL too (…/rest/v1/): only the project base URL is needed.
    Url = (builder.Configuration["Supabase:Url"] ?? "").Trim().TrimEnd('/').Replace("/rest/v1", ""),
    AnonKey = builder.Configuration["Supabase:AnonKey"] ?? "",
};
builder.Services.AddSingleton(supabase);
builder.Services.AddSingleton<SupabaseClient>();

builder.Services.AddSingleton(new TmdbOptions { ApiKey = builder.Configuration["Tmdb:ApiKey"] ?? "" });
builder.Services.AddSingleton<TmdbService>();

builder.Services.AddSingleton(new CinemaOptions
{
    Name = builder.Configuration["Cinema:Name"] ?? "",
    ScheduleUrl = builder.Configuration["Cinema:ScheduleUrl"] ?? "",
});

// Single user, single browser tab: singletons are enough.
builder.Services.AddSingleton<OutingService>();

await builder.Build().RunAsync();
