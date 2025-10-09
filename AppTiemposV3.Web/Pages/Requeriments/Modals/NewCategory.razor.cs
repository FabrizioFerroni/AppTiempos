using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Categories;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.Web.Pages.Requeriments.Modals;

public partial class NewCategory : ComponentBase
{
    [Parameter] public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private ICategoryContract<CategoryResponseDto> CategoriesService { get; set; } = null!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    
    [Parameter] public EventCallback OnSaved { get; set; }
    [Parameter] public EventCallback<string> OnCategorySelectedChanged { get; set; }
    [Parameter] public EventCallback<MarkupString> OnShowSuccess { get; set; }
    
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    
    private bool isLoadingCat = false;
    private MarkupString messageErrorCat = new("");
   
    private bool isErrorCat = false;
    
    private CreateCategoryDto categoryDto = new()
    {
        Name = null!,
        Descripcion = null,
        Color = null
    };
    
    private ElementReference closeModalCatRef;
    
    private async Task SendNewCategory()
    {
        try
        {
            isLoadingCat = true;
            StateHasChanged();
            
            GeneralResponse? response = await CategoriesService!.CreateCategory(categoryDto);
            
            if (response?.Flag == true)
            {
                await JS!.InvokeVoidAsync("modalHelpers.clickElement", closeModalCatRef);
                await OnCategorySelectedChanged.InvokeAsync(categoryDto.Name);
                await OnSaved.InvokeAsync();
                OnResetCategory();
                await OnShowSuccess.InvokeAsync(
                    (MarkupString)(response?.Message!.Replace("\n", "<br/>")!)
                );
                isLoadingCat = false;
            }
            else
            {
                isErrorCat = true;
                messageErrorCat = (MarkupString)(response?.Message?.Replace("\n", "<br />") ?? "Error desconocido");
            }
        }
        catch (Exception ex)
        {
            isErrorCat = true;
            messageErrorCat = new MarkupString(ex.Message);
        }
        finally
        {
            isLoadingCat = false;
            StateHasChanged();
        }
    }

    private void OnResetCategory()
    {
        categoryDto = new CreateCategoryDto()
        {
            Name = null!,
            Descripcion = null,
            Color = null
        };
    }
}