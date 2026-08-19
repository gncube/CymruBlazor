using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Extensions;
using CymruBlazor.Samples.Dashboard.Services;
using CymruBlazor.Samples.Dashboard;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
});

builder.Services.AddCymruBlazor();
builder.Services.AddScoped<IFocusManager, FocusManager>();
builder.Services.AddSingleton<DashboardSampleDataService>();
// Fully qualified: CymruBlazor.Theming also exposes an AddCymruBlazor() extension, which would be ambiguous with the one above.
builder.Services.AddScoped<CymruBlazor.Theming.ThemeService>();

await builder.Build().RunAsync();
