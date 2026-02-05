using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Reports;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using AppTiemposV3.Web.Pages.Reports.Modal;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;

namespace AppTiemposV3.Web.Pages.Reports
{
    public partial class Index : ComponentBase, IDisposable
    {
        #region  Variables
        #region InyeccionDependencias
        [Inject] LayoutState State { get; set; } = null!;
        [Inject] private IJSRuntime? JS { get; set; }

        [Inject] private NavigationManager? Router { get; set; }
        [Inject] private NotificationService Toltip { get; set; } = default!;
        [Inject] private ColorService ColorService { get; set; } = null!;
        [Inject] private IReportContract ReportService { get; set; } = null!;

        #endregion

        /*private List<string> OptionsSearch = new() { };
        private List<string> OptionsOrders = new() { };

        private List<int> OptionsPaginated = new() { 1, 5, 10, 15, 50 };*/

        private List<ListAllReportsDto> Reports = new();
        private int TypeSelected = 0;
        private Guid LoadingIdFav = Guid.Empty;
        private string? BuscarPor = "";

        private bool IsLoading = true;
        private bool IsLoadingDelete = false;
        private bool IsLoadingBackend = true;

        //totals....
        private int AllsCount { get; set; } = 0;
        private int FavoriteCount { get; set; } = 0;
        private int ScheduledCount { get; set; } = 0;

        private bool SearchEmpty = true;

        private int pagina = 1;
        private int registrosPorPagina = 3;
        private string ordenar = "";
        private bool ascending = true;
        private string? search { get; set; } = string.Empty;

        private string cssEnlace => 
            $"h-9 px-3 text-sm inline-flex items-center " +
            $"justify-center rounded-md font-medium transition-colors focus:outline-none " +
            $"focus:ring-0 focus:ring-offset-0 disabled:opacity-50 disabled:pointer-events-none text-gray-400  " +
            $"dark:text-gray-300 dark:border-gray-500 {ColorService.GetButtonClassess()}";

        private bool IsSelectClosed = true;

        #region Paginado
        protected int totalPages = 0;
        protected int totalElements = 0;
        protected int currentPage = 1;
        protected int maxVisiblePages = 5;
        protected bool isFirst = true;
        protected bool isLast = false;
        protected bool isFirstPage = true;
        protected bool isLastPage = false;
        #endregion

        #region Modales
        private Guid IdModal = Guid.NewGuid();
        private DeleteReport? deleteModalRef;
        #endregion
        #endregion

        #region Inicializacion
        protected override async Task OnInitializedAsync()
        {
            ColorService.OnColorChanged += HandleColorChanged;
            State.OnSidebarChanged += StateHasChanged;
            await State.InitializeAsync();
            SearchEmpty = true;
            StateHasChanged();
            await GetTotals();
            await GetAllReports(TypeSelected);
        }
        #endregion

        #region Funciones
        private async Task HandleReportDeleted()
        {
            pagina = 1;
            await GetTotals();
            await GetAllReports(TypeSelected);
        }

        private async Task GetTotals()
        {
            try
            {
                DataResponse<CountTotalReports> resp = await ReportService.GetTotalReports(search);

                if (resp.Success)
                {
                    AllsCount = resp.Data.Alls;
                    FavoriteCount = resp.Data.Favorites;
                    ScheduledCount = resp.Data.Scheduled;
                }

            } catch(Exception ex)
            {
                Console.WriteLine($"[ERROR]: error response backend: {ex.Message}");
                throw;
            }
            finally
            {

            }
        }
        private void GoToNewReport()
        {
            Router?.NavigateTo("/app/reportes/nuevo");
        }

        private void GoToEditReport(string reportUrl)
        {
            Router?.NavigateTo($"/app/reportes/editar/{reportUrl}");
        }

        private void HandleSidebarToggle()
        {
            _ = State.ToggleSidebar();
        }

        private async void HandleColorChanged()
        {
            await InvokeAsync(StateHasChanged);
        }

        private async Task DoSearch(string? query)
        {
            search = query;
            pagina = 1;

            await GetTotals();
            await GetAllReports(0);

        }

        private async Task ChangeType(int type)
        {
            TypeSelected = type;
            StateHasChanged();
            await GetAllReports(type);
        }

        private async Task AddOrRemoveFavorite(Guid id, bool result)
        {
            LoadingIdFav = id;
            StateHasChanged();
            try
            {
                AddOrRemoveFavoriteDto dto = new AddOrRemoveFavoriteDto()
                {
                    Result = result
                };

                GeneralResponse response = await ReportService.AddOrQuitFavorite(id, dto);

                if (response.Flag)
                {
                    await GetTotals();
                    await GetAllReports(TypeSelected);
                    Toltip.Success("Exito!", response.Message);
                }
                else
                {
                    await GetTotals();
                    await GetAllReports(TypeSelected);
                    Toltip.Error("Hubo un error", response.Message);
                }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                LoadingIdFav = Guid.Empty;
            }
        }

        private Task UpdateSearch()
        {
            search = "";
            SearchEmpty = false;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private async Task GetAllReports(int type = 0)
        {
            ShowLoadersLoading(true);
            StateHasChanged();

            try
            {
                
                string orderDefault = !string.IsNullOrWhiteSpace(ordenar) ? ordenar : "CreatedAt";

                PaginationDto pagination = new()
                {
                    Pagina = pagina,
                    RegistrosPorPagina = registrosPorPagina,
                    Ordenar = orderDefault,
                    Ascending = ascending,
                    Search = search
                };

                Pageable<List<ListAllReportsDto>> response = await ReportService.GetAllReports(pagination, type);

                Reports = response.Content;

                isFirst = response.First;
                isLast = response.Last;
                totalPages = response.TotalPages;
                totalElements = response.TotalElements;

                SearchEmpty = response.Content.Count < 1;
                

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                ShowLoadersLoading(false);
            }
        }


        private void ShowLoadersLoading(bool show)
        {
            IsLoading = show;
            IsLoadingBackend = show;
            StateHasChanged();
        }
        #endregion

        #region Limpiar
        public void Dispose()
        {
            ColorService.OnColorChanged -= HandleColorChanged;
            State.OnSidebarChanged -= StateHasChanged;
        }
        #endregion

        #region PaginadoFunciones
        protected async Task First()
        {
            if (!isFirst)
            {
                currentPage = 1;
                pagina = 1;
                UpdatePageStatus();
                await GetAllReports(TypeSelected);
            }
        }

        protected async Task Last()
        {
            if (!isLast)
            {
                int totalPagesCalc = (int)Math.Ceiling((double)totalElements / (registrosPorPagina == 0 ? 1 : registrosPorPagina));
                currentPage = totalPagesCalc;
                pagina = totalPagesCalc;
                UpdatePageStatus();
                await GetAllReports(TypeSelected);
            }
        }

        protected async Task Rewind()
        {
            if (currentPage > 1 && !isFirst)
            {
                currentPage--;
                pagina--;
                UpdatePageStatus();
                await GetAllReports(TypeSelected);
            }
        }

        protected async Task Forward()
        {
            if (currentPage < totalPages && !isLast)
            {
                currentPage++;
                pagina++;
                UpdatePageStatus();
                await GetAllReports(TypeSelected);
            }
        }

        protected async Task SetPage(int pageNumber)
        {
            if (pageNumber >= 1 && pageNumber <= totalPages)
            {
                currentPage = pageNumber;
                pagina = pageNumber;
                UpdatePageStatus();
                await GetAllReports(TypeSelected);
            }
        }

        private void UpdatePageStatus()
        {
            isFirst = currentPage <= 1;
            isLast = currentPage >= totalPages;
            isFirstPage = isFirst;
            isLastPage = isLast;
        }
        #endregion
        #region ModalFunciones
        private async Task DeleteModal(Guid id, string name)
        {
            await deleteModalRef!.ShowAsync(id, name);
        }
        #endregion
    }
}
