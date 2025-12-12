using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Invitations;
using AppTiemposV3.SharedClases.Enums;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;

namespace AppTiemposV3.SharedClases.Contracts;

public interface IInvitationContract<T>
{
    Task<Pageable<List<T>>> GetAllInvitations(PaginationDtoAdvanced pagination);
    
    Task<DataResponse<T>> GetInvitationPorId(Guid id);
    
    Task<DataResponse<T>> GetInvitationPorToken(string token);

    Task<GeneralResponse> CreateInvitation(CreateInvitationDto dto);

    Task<GeneralResponse> AcceptOrDeclineInvitation(Guid id, AcceptOrDeclineInvitationDto dto);
    
    Task<GeneralResponse> DeleteInvitation(Guid id);
    
    Task<DataResponse<EstadosInvitaciones>> VerifyInvitation(string token);
}