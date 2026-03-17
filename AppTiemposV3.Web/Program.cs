using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Audits;
using AppTiemposV3.SharedClases.DTOs.Categories;
using AppTiemposV3.SharedClases.DTOs.Invitations;
using AppTiemposV3.SharedClases.DTOs.RejectionDetails;
using AppTiemposV3.SharedClases.DTOs.Rejections;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.DTOs.RequerimentsAttachments;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using AppTiemposV3.SharedClases.DTOs.Users;
using AppTiemposV3.SharedClases.Utilidades;
using AppTiemposV3.Web;
using AppTiemposV3.Web.Authentication;
using AppTiemposV3.Web.Handlers;
using AppTiemposV3.Web.Services;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization;
using static AppTiemposV3.SharedClases.Utilidades.JsonOptions;



var builder = WebAssemblyHostBuilder.CreateDefault(args);



IServiceCollection? services = builder.Services;

services.AddCascadingAuthenticationState();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

services.AddScoped<AuthHeaderHandler>();

string apiBaseUrl = "#API_URL#";
string urlFinal = apiBaseUrl.StartsWith("#") ? "https://localhost:7260" : apiBaseUrl;



builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(urlFinal);
})
.AddHttpMessageHandler<AuthHeaderHandler>();

JsonSerializerOptions? jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
jsonOptions.Converters.Add(new TimeOnlyJsonConverter());
jsonOptions.Converters.Add(new JsonStringEnumConverter());
jsonOptions.Converters.Add(new FlexibleDoubleConverter());

services.AddSingleton(jsonOptions);
services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));
services.AddBlazoredLocalStorage();
services.AddBlazoredSessionStorage();
services.AddAuthorizationCore();
services.AddScoped<AuthenticationStateProvider, CustomAuthenticationProvider>();
services.AddScoped<IAuthContract, AuthService>();
services.AddScoped<IRequerimentContract<RequerimentResponseDto>, RequerimentsService>();
services.AddScoped<IAuditContract<AuditsResponseDto>, AuditService>();
services.AddScoped<IActivityContract<ActivityResponseDto>, ActivityService>();
services.AddScoped<IActivityWeeklyContract<ActivitiesByDay>, ActivityWeeklyService>();
services.AddScoped<IDashboardContract<DashboardKPIDto>, DashboardService>();
services.AddScoped<ICategoryContract<CategoryResponseDto>, CategoryService>();
services.AddScoped<ITrainingContract<TrainingResponseDto>, TrainingService>();
services.AddScoped<IRejectionContract<RejectionResponseDto>, RejectionService>();
services.AddScoped<IRejectionDetailContract<RejectionDetailResponseDto>, RejectionDetailService>();
services.AddScoped<IInvitationContract<InvitationResponseDto>, InvitationService>();
services.AddScoped<IRequerimentAttachmentContract<RequerimentsAttachmentsDto>, RequerimentAttachmentService>();
services.AddScoped<IReportContract, ReportService>();
services.AddScoped<IConfigurationContract, ConfigurationService>();
services.AddScoped<IUserCContract<UserResponseDto>, UserService>();
services.AddScoped<LayoutState>();
services.AddScoped<ColorService>();
services.AddSingleton<NotificationService>();
services.AddScoped<ActivityStateService>();

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


await js.InvokeVoidAsync("eval", $@"
(async function() {{
    try {{
        const response = await fetch('{urlFinal}/api/generics/colors');
        const data = await response.json();
        const accent = localStorage.getItem('color-accent') || 'Azul';
        const color = data.find(c => c.name?.trim().toLowerCase() === accent.trim().toLowerCase());
        const clases = (color?.gradient || 'bg-gradient-to-br from-blue-500 to-indigo-500').split(' ');
        ['logo','progressBar','dot-1','dot-2','dot-3'].forEach(id => {{
            const el = document.getElementById(id);
            if(el) el.classList.add(...clases);
        }});
    }} catch(e) {{ console.error(e); }}
}})();
");

LayoutState? layoutState = host.Services.GetRequiredService<LayoutState>();

bool? sidebarClosed = await localStorage.GetItemAsync<bool?>("SidebarClosed");
await layoutState.SetSidebar(sidebarClosed ?? false); 

await host.RunAsync();