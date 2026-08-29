using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CymruBlazor.Demo;
using CymruBlazor.Extensions;
using CymruBlazor.Accessibility.Focus;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddCymruBlazor();
builder.Services.AddScoped<IFocusManager, FocusManager>();

await builder.Build().RunAsync();
