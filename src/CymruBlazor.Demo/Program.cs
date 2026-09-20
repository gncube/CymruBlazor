using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CymruBlazor.Demo;
using CymruBlazor.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddCymruBlazor();

// Demo of localisation step 1: the current language and its strings (see /foundations/localisation).
builder.Services.AddScoped<CymruBlazor.Demo.Localisation.AppStrings>();

await builder.Build().RunAsync();
