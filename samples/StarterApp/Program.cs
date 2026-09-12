using CymruBlazor.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StarterApp;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// This one line is the only DI setup CymruBlazor needs - see
// README.md "Getting Started". No other services (focus management,
// theming, id generation, ...) need registering by hand; AddCymruBlazor()
// covers all of them.
builder.Services.AddCymruBlazor();

await builder.Build().RunAsync();
