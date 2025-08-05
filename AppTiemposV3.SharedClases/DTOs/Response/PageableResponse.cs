using AppTiemposV3.SharedClases.Annotations;

namespace AppTiemposV3.SharedClases.DTOs;

public class PageableResponse
{
    public static  Pageable<List<T>> CreatePageableResponse<T>(
        List<T> content, 
        int pageNumber, 
        int pageSize, 
        int totalElements)
    {
        int totalPages = (int)Math.Ceiling(totalElements / (double)pageSize);

        Pageable<List<T>> response = new Pageable<List<T>>
        {
            Content = content,
            
            PaginationDetails = new PageableDetails
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Offset = pageNumber * pageSize,
                NextPage =  pageNumber < totalPages ? pageNumber + 1 : pageNumber ,
                PreviousPage = pageNumber == 1 ? 1 : pageNumber - 1,
            },
            
            TotalPages = totalPages,
            TotalElements = totalElements,
            Last = pageNumber >= totalPages,
            First = pageNumber == 1,
        };

        return response;
    }
}