using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Categories;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;

namespace AppTiemposV3.Web.Pages.Requeriments.Modals;

public partial class EditRequeriment : ComponentBase
{
    public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    [Inject] private IRequerimentContract<RequerimentResponseDto> RequerimentsService { get; set; } = null!;
    [Inject] private ICategoryContract<CategoryResponseDto> CategoriesService { get; set; } = null!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    
    [Parameter] public EventCallback<SavedEventArgs> OnSaved { get; set; }

    private bool isSuccess = false;
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    private bool isErrorNReq = false;
    private MarkupString messageErrorNReq = new MarkupString("");
    private MarkupString? messageSuccess = new MarkupString("");
    private string Titulo { get; set; } = string.Empty;
    private bool IsLoadingData = true;
    private bool IsLoading = false;
    private string _cssConjuntosCambios = "flex h-10 w-full rounded-md border border-[hsl(var(--input))] bg-[hsl(var(--background))] px-3 py-2 text-[hsl(var(--base))] ring-offset-[hsl(var(--background))] file:border-0 file:bg-[hsl(var(--transparent))] file:text-sm file:font-medium file:text-[hsl(var(--foreground))] placeholder:text-[hsl(var(--muted-foreground))] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1 disabled:cursor-not-allowed disabled:opacity-50 md:text-sm font-medium bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400";
    private Dictionary<string, (string Mensaje, bool EsExitoso)> mensajes = new();
    
    private UpdateRequerimentDto requerimentUpDto = new()
    {
        Id = Guid.Empty,
        ReqID = null,
        Titulo = null,
        Cliente = null,
        CategoryId = default
    };
    
    private ElementReference showModalRef;
    private ElementReference closeModalRef;
    
    private List<Estados> OptionsEstados = Enum.GetValues(typeof(Estados))
        .Cast<Estados>()
        .Where(e => e != Estados.None)
        .ToList();
    
    private Estados? EstadoSeleccionadaNullable = null;
    
    private Estados EstadoSeleccionada
    {
        get => EstadoSeleccionadaNullable ?? default; // default es el primer valor del enum
        set => EstadoSeleccionadaNullable = value;
    }
        
    private Task OnEstadoSelectedChanged(Estados value)
    {
        EstadoSeleccionada = value;
        requerimentUpDto!.Estado = value;
        return Task.CompletedTask;
    }
    
    private List<Etapas> OptionsEtapas = Enum.GetValues(typeof(Etapas))
        .Cast<Etapas>()
        .Where(e => e != Etapas.None)
        .ToList();
    
    private Etapas? EtapaSeleccionadaNullable = Etapas.Alta;
    
    private Etapas EtapaSeleccionada
    {
        get => EtapaSeleccionadaNullable ?? default; // default es el primer valor del enum
        set => EtapaSeleccionadaNullable = value;
    }
        
    private Task OnEtapaSelectedChanged(Etapas value)
    {
        EtapaSeleccionada = value;
        requerimentUpDto!.EtapaActual = value;
        return Task.CompletedTask;
    }
    
    public async Task ShowAsync(Guid id)
    {
        Id = id;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        IsLoadingData = true;
        StateHasChanged();
        await GetAllCategories();
        await LoadDataReq(id);
    }
    
    private async Task LoadDataReq(Guid id, bool loadingData = true)
    {
        IsLoadingData = loadingData;
        StateHasChanged();
        try
        {
            DataResponse<RequerimentResponseDto>? requeriment =
                await RequerimentsService.GetRequerimentporId(id);

            requerimentUpDto.Id = (Guid)requeriment?.Data.Id!;
            requerimentUpDto.ReqID = requeriment?.Data.ReqID;
            requerimentUpDto.Titulo = requeriment?.Data.Titulo;
            Titulo = requeriment?.Data.Titulo!;
            requerimentUpDto.Cliente = requeriment?.Data.Cliente;
            requerimentUpDto.StoryPoint = requeriment?.Data.StoryPoint;
            requerimentUpDto.Url = requeriment?.Data.Url;
            requerimentUpDto.Descripcion = requeriment?.Data.Descripcion;
            requerimentUpDto.CategoryId = requeriment!.Data.Category.Id;
            requerimentUpDto.ConjuntoCambios = requeriment?.Data.ConjuntoCambios;

            await OnCategorySelectedChanged(requeriment!.Data.Category.Name);
            await OnEstadoSelectedChanged(requeriment!.Data.Estado);
            await OnEtapaSelectedChanged(requeriment!.Data.EtapaActual);
            
            StateHasChanged();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            IsLoadingData = false;
            StateHasChanged();
        }
    }

    private async Task SendEditRequeriment()
    {
        IsLoading = true;
        isErrorNReq = false;
        StateHasChanged();
        
        try {
            GeneralResponse? response = await RequerimentsService!.UpdateRequeriment(requerimentUpDto.Id, requerimentUpDto);
                
            if (response?.Flag == true)
            {
                IsLoading = false;
                
                await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalRef);

                SavedEventArgs? args = new SavedEventArgs
                {
                    Message = response.Message,
                    Success = response.Flag
                };
                await OnSaved.InvokeAsync(args);
                await OnResetEditReq(Id);
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
            IsLoading = false;
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
    
    
    private async Task OnResetEditReq(Guid id)
    {
        await LoadDataReq(id, false);
    }
    
    
    //Category seccion
    private bool IsCategoryIdSelected = false;
    private List<string> OptionsCategories = new() {};
    private Guid IdModalCategory = Guid.NewGuid();
    private readonly Guid[] IdsCategoriasCC = new Guid[]
    {
        Guid.Parse("9746cc10-b1ea-4a23-a984-3e7b04d762c1"),
    };
    private string? CategoriaSeleccionada = "";
    
    private async Task HandleCategorySaved()
    {
        await GetAllCategories(); // refrescar categorías
    }
    
    private void HandleCategorySelected(string categoryName)
    {
        OnCategorySelectedChanged(categoryName);
    }
    
    private async Task HandleCategorySuccess(MarkupString msg)
    {
        await ShowSuccess(msg);
    }
    
    private async Task ShowSuccess(MarkupString message)
    {
        messageSuccess = message;
        StateHasChanged();

        // esperar 3 segundos
        await Task.Delay(5000);

        messageSuccess = null;
        StateHasChanged();
        
        isSuccess = false;
    }
    
    public async Task GetAllCategories()
    {
        try
        {
            DataAResponse<CategoryResponseDto> categories = await CategoriesService.GetAllCategories();
            OptionsCategories = categories.Data.Select(x => x.Name).ToList()!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private Task OnCategorySelectedChanged(string value)
    {
        CategoriaSeleccionada = value;
        
        _ = SafeRunAsync(async () =>
        {
            DataResponse<Guid> categoryId = await CategoriesService.GetCategoryIdPorNombre(value);
            requerimentUpDto.CategoryId = categoryId.Data;
            IsCategoryIdSelected = true;
            StateHasChanged();
        });

        return Task.CompletedTask;
    }
    
    
    private void HandleInput(ChangeEventArgs e)
    {
        string value = e.Value?.ToString() ?? string.Empty;

        // 1️⃣ limpiar todo lo que NO sea dígito ni coma
        value = new string(value.Where(c => char.IsDigit(c) || c == ',').ToArray());

        // 2️⃣ si termina en coma → agregar cambio
        if (value.EndsWith(","))
        {
            string numero = value.TrimEnd(',');

            if (!string.IsNullOrWhiteSpace(numero))
            {
                requerimentUpDto.CambioInput = numero;
                requerimentUpDto.AddCambioFromInput();
            }

            requerimentUpDto.CambioInput = "";
            return;
        }

        // 3️⃣ seguir escribiendo normalmente
        requerimentUpDto.CambioInput = value;
    }
}