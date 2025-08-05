namespace AppTiemposV3.SharedClases.Annotations;

public class Pageable<T>
{
    public required T Content { get; set; }
    public PageableDetails? PaginationDetails { get; set; }
    public int TotalPages { get; set; }
    public int TotalElements { get; set; }
    public bool Last { get; set; }
    public bool First { get; set; }
} 


public class PageableDetails
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int Offset { get; set; }
    public int NextPage { get; set; }
    public int PreviousPage { get; set; }
}