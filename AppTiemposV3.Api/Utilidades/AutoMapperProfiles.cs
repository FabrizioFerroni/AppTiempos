using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Categories;
using AppTiemposV3.SharedClases.DTOs.Invitations;
using AppTiemposV3.SharedClases.DTOs.RejectionDetails;
using AppTiemposV3.SharedClases.DTOs.Rejections;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using AppTiemposV3.SharedClases.DTOs.Users;
using AppTiemposV3.SharedClases.Enums;
using AutoMapper;

namespace AppTiemposV3.Api.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            MappingRequeriments();
            MappingCategories();
            MappingActivities();
            MappingTraining();
            MappingInvitations();
            MappingRejections();
            MappingRejectionDetails();
            MappingUsers();
            MappingInvitation();
        }

        public void MappingRequeriments()
        {
            CreateMap<CreateRequerimentDto, RequerimentsEntity>();
            CreateMap<UpdateRequerimentDto, RequerimentsEntity>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.HasValue ? (Estados?)src.Estado.Value : null))
                .ForMember(dest => dest.EtapaActual, opt => opt.MapFrom(src => src.EtapaActual.HasValue ? (Etapas?)src.EtapaActual.Value : null))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<RequerimentsEntity, RequerimentResponseDto>()
                .ForMember(d => d.Usuario, o => o.MapFrom(s => s.User))
                .ForMember(d => d.Category, o => o.MapFrom(s => s.Category))
                // .ForMember(dest => dest.Etapa, opt => opt.MapFrom(src => src.Etapa.HasValue ? (Estados?)src.Etapa.Value : null))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado));
         
            CreateMap<CategoriesEntity, CategoryDtoA>();
            CreateMap<UserEntity, UserDtoR>();
        }

        public void MappingCategories()
        {
            CreateMap<CreateCategoryDto, CategoriesEntity>();
            CreateMap<UpdateCategoryDto, CategoriesEntity>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CategoriesEntity, CategoryResponseDto>();
        }

        public void MappingActivities()
        {
            CreateMap<CreateActivityDto, ActivitiesEntity>();
            
            CreateMap<UpdateActivityDto, ActivitiesEntity>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            CreateMap<ActivitiesEntity, ActivityResponseDto>()
                .ForMember(d => d.Requeriment, o => o.MapFrom(s => s.Requeriment))
                .ForMember(d => d.Usuario, o => o.MapFrom(s => s.User))
                .ForMember(dest => dest.Etapa, opt => opt.MapFrom(src => src.Etapa))
                .ForMember(dest => dest.StartTime,
                    opt => opt.MapFrom(src => src.StartTime.ToString("HH:mm")))
                .ForMember(dest => dest.EndTime,
                    opt => opt.MapFrom(src => src.EndTime.HasValue ? src.EndTime.Value.ToString("HH:mm") : null));

            CreateMap<RequerimentsEntity, RequerimentDtoA>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
            
            CreateMap<UserEntity, UserDtoA>();
            CreateMap<CategoriesEntity, CategoryDtoRes>();
            //TODO: Luego ver esto
            CreateMap<Etapas, Etapas>().ConvertUsing(src => (Etapas)src);
            CreateMap<Etapas?, Etapas?>().ConvertUsing(src => src.HasValue ? (Etapas?)src.Value : null);
        }

        public void MappingTraining()
        {
            CreateMap<CreateTrainingDto, TrainingEntity>();
            
            CreateMap<UpdateTrainingDto, TrainingEntity>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            CreateMap<TrainingEntity, TrainingResponseDto>()
                .ForMember(d => d.Requeriment, o => o.MapFrom(s => s.Requeriment))
                .ForMember(d => d.Usuario, o => o.MapFrom(s => s.User))
                .ForMember(dest => dest.StartTime,
                    opt => opt.MapFrom(src => src.StartTime.ToString("HH:mm")))
                .ForMember(dest => dest.EndTime,
                    opt => opt.MapFrom(src => src.EndTime.HasValue ? src.EndTime.Value.ToString("HH:mm") : null));

            CreateMap<RequerimentsEntity, RequerimentDtoT>();
            CreateMap<UserEntity, UserDtoT>();
        }

        public void MappingInvitations()
        {
            CreateMap<InviteDto, InvitationEntity>();
        }

        public void MappingRejections()
        {
            CreateMap<CreateRejectionDto, RejectionEntity>();
            CreateMap<UpdateRejectionDto, RejectionEntity>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<RejectionEntity, RejectionResponseDto>()
                .ForMember(d => d.Requeriment, o => o.MapFrom(s => s.Requeriment))
                .ForMember(d => d.Usuario, o => o.MapFrom(s => s.User))
                .ForMember(d => d.RejectionsDetails, o => o.MapFrom(s => s.RejectionsDetails));
            
            CreateMap<RequerimentsEntity, RequerimentDtoRej>();
            CreateMap<UserEntity, UserDtoARej>();
            CreateMap<CategoriesEntity, CategoryDtoResRej>();
        }

        public void MappingRejectionDetails()
        {
            CreateMap<CreateRejectionDetailDto, RejectionDetailEntity>();
            CreateMap<UpdateRejectionDetailDto, RejectionDetailEntity>();

            CreateMap<RejectionDetailEntity, RejectionDetailResponseDto>();
            
            CreateMap<RequerimentsEntity, RequerimentDtoRej>();
        }

        public void MappingUsers()
        {
            CreateMap<UserEntity, UserResponseDto>();
            CreateMap<UpdateUserDto, UserEntity>();
            CreateMap<UpdatePasswordUserDto, UserEntity>();
        }

        private void MappingInvitation()
        {
            CreateMap<CreateInvitationDto, InvitationEntity>();
            CreateMap<InvitationEntity, InvitationResponseDto>();
            CreateMap<AcceptOrDeclineInvitationDto, InvitationEntity>();
        }
    }
}
