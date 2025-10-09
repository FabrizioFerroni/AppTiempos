using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;
using static NanoidDotNet.Nanoid.Alphabets;

namespace AppTiemposV3.Web.Pages.Requeriments.Documents;

public partial class DocumentModal : ComponentBase
{
    public Guid Id { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }
    private string IdModalStr = Generate(LowercaseLettersAndDigits, 10);
    
    private ElementReference showModalRef;
    private ElementReference closeModalRef;

    private int countData = 0;
    private bool IsDisabled = true; //TODO: Borrar esto
    
    public async Task ShowAsync(Guid id)
    {
        //Hay que pasarle el id del req porque va atado al req esto.
        Id = id;
        await JS!.InvokeVoidAsync("modalHelpers.clickElement", showModalRef);
        StateHasChanged();
    }
}