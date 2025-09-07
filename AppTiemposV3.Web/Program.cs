using AppTiemposV3.SharedClases.Contracts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AppTiemposV3.Web;
using AppTiemposV3.Web.Authentication;
using AppTiemposV3.Web.Services;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;


var builder = WebAssemblyHostBuilder.CreateDefault(args);

IServiceCollection? services = builder.Services;

services.AddCascadingAuthenticationState();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7260") });
services.AddBlazoredLocalStorage();
services.AddBlazoredSessionStorage();
services.AddAuthorizationCore();
services.AddScoped<AuthenticationStateProvider, CustomAuthenticationProvider>();
services.AddScoped<IAuthContract, AuthService>();

//await builder.Build().RunAsync();
WebAssemblyHost? host = builder.Build();

ILocalStorageService? localStorage = host.Services.GetRequiredService<ILocalStorageService>();
string? theme = await localStorage.GetItemAsync<string>("color-theme");

if (theme == "dark")
{
    await host.Services.GetRequiredService<IJSRuntime>()
        .InvokeVoidAsync("eval", "document.documentElement.classList.add('dark')");
}
else
{
    await host.Services.GetRequiredService<IJSRuntime>()
        .InvokeVoidAsync("eval", "document.documentElement.classList.remove('dark')");
}

IJSRuntime? js = host.Services.GetRequiredService<IJSRuntime>();

await js.InvokeVoidAsync("eval", @"
    const script = document.createElement('script');
    script.src = 'https://cdn.jsdelivr.net/npm/@tailwindplus/elements@1';
    script.type = 'module';
    document.head.appendChild(script);
");

await host.RunAsync();