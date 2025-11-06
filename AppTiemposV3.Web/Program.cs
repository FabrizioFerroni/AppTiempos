using System.Text.Json;
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Categories;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.Utilidades;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AppTiemposV3.Web;
using AppTiemposV3.Web.Authentication;
using AppTiemposV3.Web.Handlers;
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

services.AddScoped<AuthHeaderHandler>();
//services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7260") });
services.AddHttpClient("API", client =>
    {
        client.BaseAddress = new Uri("https://localhost:7260");
    })
    .AddHttpMessageHandler<AuthHeaderHandler>();

JsonSerializerOptions? jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
jsonOptions.Converters.Add(new TimeOnlyJsonConverter());
jsonOptions.Converters.Add(new JsonStringEnumConverter());

services.AddSingleton(jsonOptions);
services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));
services.AddBlazoredLocalStorage();
services.AddBlazoredSessionStorage();
services.AddAuthorizationCore();
services.AddScoped<AuthenticationStateProvider, CustomAuthenticationProvider>();
services.AddScoped<IAuthContract, AuthService>();
services.AddScoped<IRequerimentContract<RequerimentResponseDto>, RequerimentsService>();
services.AddScoped<IActivityContract<ActivityResponseDto>, ActivityService>();
services.AddScoped<IActivityWeeklyContract<ActivitiesByDay>, ActivityWeeklyService>();
services.AddScoped<ICategoryContract<CategoryResponseDto>, CategoryService>();
services.AddScoped<LayoutState>();
services.AddScoped<ColorService>();
services.AddSingleton<NotificationService>();

WebAssemblyHost? host = builder.Build();

ColorService? colorService = host.Services.GetRequiredService<ColorService>();
await colorService.InitializeAsync();

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

await js.InvokeVoidAsync("eval", @"
    const script = document.createElement('script');
    script.src = '/js/clipboardToUrl.js';
    script.type = 'text/javascript';
    document.head.appendChild(script);
");

await js.InvokeVoidAsync("eval", @"
    const script = document.createElement('script');
    script.src = '/js/modalHelpers.js';
    script.type = 'text/javascript';
    document.head.appendChild(script);
");

await js.InvokeVoidAsync("eval", @"
    const script = document.createElement('script');
    script.src = '/js/dateTimeHelper.js';
    script.type = 'text/javascript';
    document.head.appendChild(script);
");

await js.InvokeVoidAsync("eval", @"
    const script = document.createElement('script');
    script.src = '/js/site.js';
    script.type = 'text/javascript';
    document.head.appendChild(script);
");

await js.InvokeVoidAsync("eval", @"
    const script = document.createElement('script');
    script.src = '/js/selectHelper.js';
    script.type = 'text/javascript';
    document.head.appendChild(script);
");

await js.InvokeVoidAsync("eval", @"
    const script = document.createElement('script');
    script.src = '/js/utils.js';
    script.type = 'text/javascript';
    document.head.appendChild(script);
");

await js.InvokeVoidAsync("eval", @"
(async function() {
    try {
        const response = await fetch('https://localhost:7260/api/generics/colors');
        const data = await response.json();
        const accent = localStorage.getItem('color-accent') || 'Azul';
        const color = data.find(c => c.name?.trim().toLowerCase() === accent.trim().toLowerCase());
        const clases = (color?.gradient || 'bg-gradient-to-br from-blue-500 to-indigo-500').split(' ');
        ['logo','progressBar','dot-1','dot-2','dot-3'].forEach(id => {
            const el = document.getElementById(id);
            if(el) el.classList.add(...clases);
        });
    } catch(e) { console.error(e); }
})();
");

LayoutState? layoutState = host.Services.GetRequiredService<LayoutState>();

bool? sidebarClosed = await localStorage.GetItemAsync<bool?>("SidebarClosed");
await layoutState.SetSidebar(sidebarClosed ?? false); 

await host.RunAsync();