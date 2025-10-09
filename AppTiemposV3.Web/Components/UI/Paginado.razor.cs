using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class Paginado<T> : ComponentBase, IDisposable
{
    [Parameter] public List<T> Content { get; set; } = new();
    [Inject] private ColorService color { get; set; } = null!;
    [Parameter] public int PageNumber { get; set; }
    [Parameter] public int Size { get; set; } = 10;
    [Parameter] public int TotalElements { get; set; }
    [Parameter] public int CurrentPage { get; set; }
    [Parameter] public int TotalPages { get; set; }
    [Parameter] public int MaxVisiblePages { get; set; } = 5;
    [Parameter] public bool IsFirst { get; set; }
    [Parameter] public bool IsLast { get; set; }
    [Parameter] public bool ShowCantPages { get; set; } = true;

    [Parameter] public EventCallback FirstPage { get; set; }
    [Parameter] public EventCallback LastPage { get; set; }
    [Parameter] public EventCallback RewindPage { get; set; }
    [Parameter] public EventCallback ForwardPage { get; set; }
    [Parameter] public EventCallback<int> OnSetPage { get; set; }

    private async Task First() { if (!IsFirst) await FirstPage.InvokeAsync(); }
    private async Task Last() { if (!IsLast) await LastPage.InvokeAsync(); }
    private async Task Rewind() { if (CurrentPage > 1 && !IsFirst) await RewindPage.InvokeAsync(); }
    private async Task Forward() { if (CurrentPage < TotalPages && !IsLast) await ForwardPage.InvokeAsync(); }
    private async Task SetPage(int pageNumber) { if (pageNumber >= 1 && pageNumber <= TotalPages) await OnSetPage.InvokeAsync(pageNumber); }

    protected override async Task OnInitializedAsync()
    {
        color.OnColorChanged += HandleColorChanged;
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }
   
   private string GetShowingResultsMessage()
   {
       if (TotalElements == 0)
           return "No hay registros para mostrar";

       if (TotalElements == 1)
           return "Mostrando 1 registro";

       // Caso especial: todos los registros entran en la primera página
       if (PageNumber == 1 && Size >= TotalElements)
           return $"Mostrando registros del {TotalElements} al {TotalElements} de un total de {TotalElements} registros";

       int startIndex = (PageNumber - 1) * Size + 1;
       int endIndex = Math.Min(PageNumber * Size, TotalElements);

       return $"Mostrando registros del {startIndex} al {endIndex} de un total de {TotalElements} registros";
   }

   
    private IEnumerable<int> VisiblePagesBefore
    {
        get
        {
            int currentPageIndex = CurrentPage - 1;
            int[] totalPagesArray = Enumerable.Range(1, TotalPages).ToArray();
            int leftIndex = Math.Max(0, currentPageIndex - MaxVisiblePages);
            List<int> visiblePages = totalPagesArray.Skip(leftIndex).Take(currentPageIndex - leftIndex).ToList();

            if (leftIndex > 0) visiblePages.Insert(0, 0);
            return visiblePages;
        }
    }

    private IEnumerable<int> VisiblePagesAfter
    {
        get
        {
            int currentPageIndex = CurrentPage - 1;
            int[] totalPagesArray = Enumerable.Range(1, TotalPages).ToArray();
            int rightIndex = Math.Min(TotalPages, currentPageIndex + MaxVisiblePages);
            List<int> visiblePages = totalPagesArray.Skip(currentPageIndex).Take(rightIndex - currentPageIndex).ToList();

            if (rightIndex < TotalPages) visiblePages.Add(0);
            return visiblePages;
        }
    }
    
    private IEnumerable<object> GetPageNumbers()
    {
        List<object> pages = new List<object>();

        if (TotalPages <= MaxVisiblePages)
        {
            // Mostrar todas las páginas
            for (int i = 1; i <= TotalPages; i++)
                pages.Add(i);
        }
        else
        {
            int left = Math.Max(1, CurrentPage - 2);
            int right = Math.Min(TotalPages, CurrentPage + 2);

            // Siempre mostramos la primera página
            pages.Add(1);

            // Si hay hueco entre la 1 y la izquierda, ponemos ...
            if (left > 2)
                pages.Add("...");

            // Páginas del bloque central
            for (int i = left; i <= right; i++)
            {
                if (i != 1 && i != TotalPages)
                    pages.Add(i);
            }

            // Si hay hueco entre la derecha y la última, ponemos ...
            if (right < TotalPages - 1)
                pages.Add("...");

            // Siempre mostramos la última
            if (TotalPages > 1)
                pages.Add(TotalPages);
        }

        return pages;
    }
    
    public void Dispose()
    {
        color.OnColorChanged -= HandleColorChanged; 
    }
}