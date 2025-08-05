using AppTiemposV3.Api.Entities;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Categories;
using AppTiemposV3.SharedClases.DTOs.Requeriments;
using AppTiemposV3.SharedClases.DTOs.Trainings;
using AutoMapper;
using Tipo = AppTiemposV3.Api.Entities.Tipo;

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
        }

        public void MappingRequeriments()
        {
            CreateMap<CreateRequerimentDto, RequerimentsEntity>();
            CreateMap<UpdateRequerimentDto, RequerimentsEntity>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<RequerimentsEntity, RequerimentResponseDto>()
                .ForMember(d => d.Usuario, o => o.MapFrom(s => s.User));

            CreateMap<UserEntity, UserDto>();
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
                .ForMember(d => d.Category, o => o.MapFrom(s => s.Category))
                .ForMember(d => d.Usuario, o => o.MapFrom(s => s.User));

            CreateMap<RequerimentsEntity, RequerimentDtoA>()
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo.HasValue ? (TipoA?)src.Tipo.Value : null));
            CreateMap<CategoriesEntity, CategoryDtoA>();
            CreateMap<UserEntity, UserDtoA>();
            CreateMap<Tipo, TipoA>().ConvertUsing(src => (TipoA)src);
            CreateMap<Tipo?, TipoA?>().ConvertUsing(src => src.HasValue ? (TipoA?)src.Value : null);
        }

        public void MappingTraining()
        {
            CreateMap<CreateTrainingDto, TrainingEntity>();
            
            CreateMap<UpdateTrainingDto, TrainingEntity>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            CreateMap<TrainingEntity, TrainingResponseDto>()
                .ForMember(d => d.Requeriment, o => o.MapFrom(s => s.Requeriment))
                .ForMember(d => d.Category, o => o.MapFrom(s => s.Category))
                .ForMember(d => d.Usuario, o => o.MapFrom(s => s.User));

            CreateMap<RequerimentsEntity, RequerimentDtoT>();
            CreateMap<CategoriesEntity, CategoryDtoT>();
            CreateMap<UserEntity, UserDtoT>();
        }
    }
}
