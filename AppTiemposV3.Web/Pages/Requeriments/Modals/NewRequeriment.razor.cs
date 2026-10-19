using AppTiemposV3.SharedClases.Constants;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Categories;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.Web.Services;
using AppTiemposV3.SharedClases.Policies;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.Policies.EtapaPolicy;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;

namespace AppTiemposV3.Web.Pages.Requeriments.Modals;

public partial class NewRequeriment : ComponentBase
{
    //#region Inyeccion de dependencias
    [Inject] private IJSRuntime? JS { get; set; }

    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private IRequerimentContract<RequerimentResponseDto> RequerimentsService { get; set; } = null!;
    [Inject] private ICategoryContract<CategoryResponseDto> CategoriesService { get; set; } = null!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;
    //#endregion

    //#region parametros configurables
    [Parameter] public Guid Id { get; set; }
    [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }

    [Parameter] public EventCallback<string> OnRequerimentSelectedChanged { get; set; }
    [Parameter] public EventCallback<MarkupString> OnShowSuccess { get; set; }
    [Parameter] public EventCallback<Guid> OnRequerimentExistente { get; set; }
    //#endregion

    //#region Variables
    private MarkupString? messageSuccessCat = new MarkupString("");
    private bool isSuccess = false;

    private Guid IdModalCategory = Guid.NewGuid();

    private string? CategoriaSeleccionada = "";
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    private bool isErrorNReq = false;
    private MarkupString messageErrorNReq = new("");
    private CreateRequerimentDto requerimentDto = new()
    {
        ReqID = null,
        Titulo = null,
        Cliente = null,
        CategoryId = default
    };
    private List<string> OptionsCategories = new() { };
    private bool IsCategoryIdSelected = false;
    private bool IsLoadingNew = false;
    private string _cssConjuntosCambios = "flex h-10 w-full rounded-md border border-[hsl(var(--input))] bg-[hsl(var(--background))] px-3 py-2 text-[hsl(var(--base))] ring-offset-[hsl(var(--background))] file:border-0 file:bg-[hsl(var(--transparent))] file:text-sm file:font-medium file:text-[hsl(var(--foreground))] placeholder:text-[hsl(var(--muted-foreground))] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 disabled:cursor-not-allowed disabled:opacity-50 md:text-sm font-medium bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400";

    private Dictionary<string, (string Mensaje, bool EsExitoso)> mensajes = new();

    private ElementReference showModalRef;
    private ElementReference closeModalRef;

    private Areas _areaUsuario = Areas.None;

    private static readonly HashSet<string> SlugsIncidente = new(StringComparer.OrdinalIgnoreCase)
    {
        "incidente-no-critico",
        "incidente-externo",
        "incidente-critico",
        "incidente-de-usuario",
        "incidente-de-implementacion",
        "incidente-interno",
        "incidente-de-configuracion",
    };

    private List<CategoryResponseDto> _categories = new();
    private string? _slugCategoriaSeleccionada;

    private readonly List<Etapas> OptionsEtapas = Enum.GetValues<Etapas>()
    .Where(e => e != Etapas.None)
    .ToList();

    private Etapas EtapaSeleccionada { get; set; }
    private bool _etapaEditadaManualmente;
    private bool EsIncidente =>
    _slugCategoriaSeleccionada is not null && SlugsIncidente.Contains(_slugCategoriaSeleccionada);

    private const string StoryPointEvaluacion = "0.00";

    private bool EsEvaluacion => EtapaSeleccionada == Etapas.Evaluacion;
    //#endregion

    //#region Inicializacion
    protected override async Task OnInitializedAsync()
    {
        AuthenticationState state = await AuthStateProvider.GetAuthenticationStateAsync();
        string? claim = state.User.FindFirst(CustomClaimTypes.Area)?.Value;

        if (Enum.TryParse(claim, ignoreCase: true, out Areas area) && Enum.IsDefined(area))
            _areaUsuario = area;

        AplicarReglaDeEtapa();
    }
    //#endregion



    public async Task ShowAsync(Guid id)
    {
        Id = id;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        StateHasChanged();
        await GetAllCategories();
    }


    private async Task OnReqIdChanged(int newReqId)
    {
        string reqId = newReqId.ToString();
        requerimentDto.ReqID = reqId;

        try
        {
            DataResponse<Guid?> existente = await RequerimentsService.FindIdByReqId(reqId);

            if (existente.Data is Guid idExistente && idExistente != Guid.Empty)
            {
                OnResetNewReq();
                await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);
                await OnRequerimentExistente.InvokeAsync(idExistente);
                return;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error verificando ReqID: {ex}");
        }

        StateHasChanged();
    }

    private async Task HandleCategorySaved()
    {
        await GetAllCategories();
    }

    private void HandleCategorySelected(string categoryName)
    {
        OnCategorySelectedChanged(categoryName);
    }

    private async Task HandleCategorySuccess(MarkupString msg)
    {
        await ShowSuccess(msg);
    }

    private void AplicarReglaDeEtapa()
    {
        if (!_etapaEditadaManualmente)
        {
            Etapas sugerida = ObtenerDefault(_areaUsuario, EsIncidente);
            EtapaSeleccionada = sugerida;
            requerimentDto.EtapaActual = sugerida;
            _etapaEditadaManualmente = false;
        }

        SincronizarStoryPoint();
    }

    public async Task GetAllCategories()
    {
        try
        {
            DataAResponse<CategoryResponseDto> categories = await CategoriesService.GetAllCategories();
            OptionsCategories = categories.Data.Select(x => x.Name).ToList()!;
            _categories = categories.Data.ToList()!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void SincronizarStoryPoint()
    {
        if (EsEvaluacion)
        {
            requerimentDto.StoryPoint = StoryPointEvaluacion;
        }
        else if (requerimentDto.StoryPoint == StoryPointEvaluacion)
        {
            requerimentDto.StoryPoint = null!;
        }
    }


    private Task OnCategorySelectedChanged(string value)
    {
        CategoriaSeleccionada = value;
        _slugCategoriaSeleccionada = _categories.FirstOrDefault(c => c.Name == value)?.Slug;
        AplicarReglaDeEtapa();

        _ = SafeRunAsync(async () =>
        {
            DataResponse<Guid> categoryId = await CategoriesService.GetCategoryIdPorNombre(value);
            requerimentDto.CategoryId = categoryId.Data;
            IsCategoryIdSelected = true;
            StateHasChanged();
        });

        return Task.CompletedTask;
    }

    private async Task SendNewRequeriment()
    {
        try
        {
            IsLoadingNew = true;
            isErrorNReq = false;
            StateHasChanged();

            GeneralResponse? response = await RequerimentsService!.CreateRequeriment(requerimentDto);

            if (response?.Flag == true)
            {
                OnResetNewReq();
                await OnRequerimentSelectedChanged.InvokeAsync(requerimentDto.ReqID);
                await OnShowSuccess.InvokeAsync(
                    (MarkupString)(response?.Message!.Replace("\n", "<br/>")!)
                );
                IsLoadingNew = false;
                await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);
                SavedEventArgs? args = new SavedEventArgs
                {
                    Message = response.Message,
                    Success = response.Flag
                };
                await OnSaved.InvokeAsync(args);
            }
            else
            {
                isErrorNReq = true;
                messageErrorNReq = (MarkupString)(response?.Message?.Replace("\n", "<br />") ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            isErrorNReq = true;
            messageErrorNReq = new MarkupString(ex.Message);
        }
        finally
        {
            IsLoadingNew = false;
            StateHasChanged();
        }
    }

    private async Task SafeRunAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error en fire & forget: {ex}");
        }
    }

    private async Task ShowSuccess(MarkupString message)
    {
        messageSuccessCat = message;
        StateHasChanged();

        await Task.Delay(5000);

        messageSuccessCat = null;
        StateHasChanged();

        isSuccess = false;
    }

    private void OnResetNewReq()
    {
        CategoriaSeleccionada = "";
        _slugCategoriaSeleccionada = null;
        requerimentDto = new()
        {
            Titulo = null!,
            Descripcion = null,
            Cliente = null!,
            ReqID = null!,
            CategoryId = default,
            ConjuntoCambios = null!,
            Url = null,
            StoryPoint = null!
        };
        _etapaEditadaManualmente = false;
        AplicarReglaDeEtapa();
    }

    private void HandleInput(ChangeEventArgs e)
    {
        string value = e.Value?.ToString() ?? string.Empty;

        value = new string(value.Where(c => char.IsDigit(c) || c == ',').ToArray());

        if (value.EndsWith(","))
        {
            string numero = value.TrimEnd(',');

            if (!string.IsNullOrWhiteSpace(numero))
            {
                requerimentDto.CambioInput = numero;
                requerimentDto.AddCambioFromInput();
            }

            requerimentDto.CambioInput = "";
            return;
        }

        requerimentDto.CambioInput = value;
    }


    private Task OnEtapaSelectedChanged(Etapas value)
    {
        _etapaEditadaManualmente = true;
        EtapaSeleccionada = value;
        requerimentDto.EtapaActual = value;
        SincronizarStoryPoint();
        return Task.CompletedTask;
    }
}