using AppTiemposV3.SharedClases.Contracts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AppTiemposV3.Web;
using AppTiemposV3.Web.Authentication;
using AppTiemposV3.Web.Services;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;


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

await builder.Build().RunAsync();