using System.Linq.Expressions;
using System.Reflection;
using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.Api.Utilidades;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.Exceptions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppTiemposV3.Api.Repositories;

public class GenericRepository : IGenericContract
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly UserManager<UserEntity> _userManager;

    public GenericRepository(AppDbContext dbContext, IMapper mapper, UserManager<UserEntity> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _userManager = userManager;
    }
    
    public async Task<Pageable<List<TDto>>> GetAllPaginatedAsync<TEntity, TDto>(PaginationDto pagination, string buscarPor, Guid? userId) where TEntity : class
    {
        // Buscar propiedad por string (search)
        ParameterExpression paramBuscar = Expression.Parameter(typeof(TEntity), "e");
        MemberExpression propertyBuscar = Expression.Property(paramBuscar, buscarPor);
        Expression<Func<TEntity, object>> expressionBuscar = Expression.Lambda<Func<TEntity, object>>(
            Expression.Convert(propertyBuscar, typeof(object)), paramBuscar
        );
        
        IQueryable<TEntity> queryable = _dbContext.Set<TEntity>().AsQueryable();

        // Buscar user
        if (userId is not null && userId != Guid.Empty)
        {
            UserEntity user = await GetUserByIdAsync(userId);
            // Filtro por UserId (si existe la propiedad)
            PropertyInfo? userIdProperty = typeof(TEntity).GetProperty("UserId");
            if (userIdProperty != null)
            {
                ParameterExpression paramUser = Expression.Parameter(typeof(TEntity), "e");
                MemberExpression userIdProp = Expression.Property(paramUser, "UserId");
                ConstantExpression userIdValue = Expression.Constant(user.Id);
                BinaryExpression equalUser = Expression.Equal(userIdProp, userIdValue);
                Expression<Func<TEntity, bool>> userFilter = Expression.Lambda<Func<TEntity, bool>>(equalUser, paramUser);
                queryable = queryable.Where(userFilter);
            }
        }
        
        // Filtro por búsqueda
        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            string propertyName;
            if (expressionBuscar.Body is UnaryExpression unaryExp && unaryExp.Operand is MemberExpression memberExp)
                propertyName = memberExp.Member.Name;
            else if (expressionBuscar.Body is MemberExpression directMemberExp)
                propertyName = directMemberExp.Member.Name;
            else
                throw new InvalidOperationException("No se pudo resolver la propiedad de búsqueda.");

            ParameterExpression param = Expression.Parameter(typeof(TEntity), "e");
            MemberExpression propertyAccess = Expression.Property(param, propertyName);
            Type propertyType = propertyAccess.Type;

            Expression<Func<TEntity, bool>> lambda;

            // Detectamos el tipo del campo dinámicamente
            if (propertyType == typeof(string))
            {
                // Campo string → usar Contains
                MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                ConstantExpression searchValue = Expression.Constant(pagination.Search);
                MethodCallExpression containsExpression = Expression.Call(propertyAccess, containsMethod, searchValue);
                lambda = Expression.Lambda<Func<TEntity, bool>>(containsExpression, param);
            }
            else if (propertyType == typeof(Guid))
            {
                // Campo Guid → comparar por igualdad si se puede parsear
                if (Guid.TryParse(pagination.Search, out Guid guidValue))
                {
                    ConstantExpression searchGuid = Expression.Constant(guidValue);
                    BinaryExpression equal = Expression.Equal(propertyAccess, searchGuid);
                    lambda = Expression.Lambda<Func<TEntity, bool>>(equal, param);
                }
                else
                {
                    // Si no es un GUID válido, que no devuelva nada
                    lambda = e => false;
                }
            }
            else if (IsNumericType(propertyType))
            {
                // Campo numérico → comparar igualdad si se puede convertir
                if (decimal.TryParse(pagination.Search, out decimal numValue))
                {
                    ConstantExpression searchNum = Expression.Constant(Convert.ChangeType(numValue, propertyType));
                    BinaryExpression equal = Expression.Equal(propertyAccess, searchNum);
                    lambda = Expression.Lambda<Func<TEntity, bool>>(equal, param);
                }
                else
                {
                    lambda = e => false;
                }
            }
            else
            {
                // Otros tipos (ej. DateTime, Enum, etc.) → comparar con ToString().Contains()
                MethodInfo toStringMethod = propertyType.GetMethod("ToString", Type.EmptyTypes)!;
                MethodCallExpression toStringCall = Expression.Call(propertyAccess, toStringMethod);
                MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                ConstantExpression searchValue = Expression.Constant(pagination.Search);
                MethodCallExpression containsExpression = Expression.Call(toStringCall, containsMethod, searchValue);
                lambda = Expression.Lambda<Func<TEntity, bool>>(containsExpression, param);
            }

            queryable = queryable.Where(lambda);
        }



        // Total
        int totalElements = await queryable.CountAsync();

        // Ordenamiento
        if (!string.IsNullOrEmpty(pagination.Ordenar))
        {
            queryable = pagination.Ascending
                ? queryable.OrderBy(e => EF.Property<object>(e, pagination.Ordenar))
                : queryable.OrderByDescending(e => EF.Property<object>(e, pagination.Ordenar));
        }
        else
        {
            queryable = pagination.Ascending
                ? queryable.OrderBy(e => EF.Property<DateTime>(e, "CreatedAt"))
                : queryable.OrderByDescending(e => EF.Property<DateTime>(e, "CreatedAt"));
        }

        // Paginación + proyección al DTO
        List<TDto> resDto = await queryable
            .Paginar(pagination)
            .ProjectTo<TDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        Pageable<List<TDto>> response = PageableResponse.CreatePageableResponse(
            resDto,
            pagination.Pagina,
            pagination.RegistrosPorPagina,
            totalElements
        );

        return response;
    }
    
     public async Task<Pageable<List<TDto>>> GetAllPaginatedPerDayAsync<TEntity, TDto>(PaginationDto pagination, DateOnly startDate, Guid? userId) where TEntity : class
     {
        IQueryable<TEntity> queryable = _dbContext.Set<TEntity>();

        // Filtro por UserId (si la propiedad existe)
        if (userId is not null && userId != Guid.Empty)
        {
            UserEntity user = await GetUserByIdAsync(userId);
            PropertyInfo? userIdProperty = typeof(TEntity).GetProperty("UserId");
            if (userIdProperty != null)
            {
                ParameterExpression param = Expression.Parameter(typeof(TEntity), "e");
                MemberExpression userIdProp = Expression.Property(param, "UserId");
                ConstantExpression userIdValue = Expression.Constant(user.Id);
                BinaryExpression equal = Expression.Equal(userIdProp, userIdValue);
                Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(equal, param);
                queryable = queryable.Where(lambda);
            }
        }

        // Filtro por StartDate
        {
            PropertyInfo? startDateProp = typeof(TEntity).GetProperty("StartDate");
            if (startDateProp != null && startDateProp.PropertyType == typeof(DateOnly))
            {
                ParameterExpression param = Expression.Parameter(typeof(TEntity), "e");
                MemberExpression propAccess = Expression.Property(param, "StartDate");
                ConstantExpression dateValue = Expression.Constant(startDate);
                BinaryExpression equal = Expression.Equal(propAccess, dateValue);
                Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(equal, param);
                queryable = queryable.Where(lambda);
            }
            else
            {
                throw new InvalidOperationException("La entidad no contiene una propiedad StartDate de tipo DateOnly.");
            }
        }

        // Total elementos
        int totalElements = await queryable.CountAsync();

        // Ordenamiento
        if (pagination.Ordenar == "StartDateTimeCombo")
        {
            queryable = queryable
                .OrderByDescending(e => EF.Property<DateOnly>(e, "StartDate"))
                .ThenByDescending(e => EF.Property<TimeOnly>(e, "StartTime"));
        }
        else
        {
            // Comportamiento estándar
            queryable = !string.IsNullOrEmpty(pagination.Ordenar)
                ? (pagination.Ascending
                    ? queryable.OrderBy(e => EF.Property<object>(e, pagination.Ordenar))
                    : queryable.OrderByDescending(e => EF.Property<object>(e, pagination.Ordenar)))
                : (pagination.Ascending
                    ? queryable.OrderBy(e => EF.Property<DateTime>(e, "CreatedAt"))
                    : queryable.OrderByDescending(e => EF.Property<DateTime>(e, "CreatedAt")));
        }

        // Paginado + mapeo
        List<TDto> resDto = await queryable
            .Paginar(pagination)
            .ProjectTo<TDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return PageableResponse.CreatePageableResponse(
            resDto,
            pagination.Pagina,
            pagination.RegistrosPorPagina,
            totalElements
        );
     }
     
      public async Task<Pageable<List<TDto>>> GetAllPaginatedPerRangeAsync<TEntity, TDto>(PaginationDto pagination, DateOnly startDate, DateOnly? endDate, Guid? userId) where TEntity : class
    {
        IQueryable<TEntity> queryable = _dbContext.Set<TEntity>();

        // Filtro por UserId (si la propiedad existe)
        if (userId is not null && userId != Guid.Empty)
        {
            UserEntity user = await GetUserByIdAsync(userId);
            PropertyInfo? userIdProperty = typeof(TEntity).GetProperty("UserId");
            if (userIdProperty != null)
            {
                ParameterExpression param = Expression.Parameter(typeof(TEntity), "e");
                MemberExpression userIdProp = Expression.Property(param, "UserId");
                ConstantExpression userIdValue = Expression.Constant(user.Id);
                BinaryExpression equal = Expression.Equal(userIdProp, userIdValue);
                Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(equal, param);
                queryable = queryable.Where(lambda);
            }
        }

        // Filtro por rango de fechas (StartDate)
        {
            PropertyInfo? startDateProp = typeof(TEntity).GetProperty("StartDate");
            if (startDateProp != null && startDateProp.PropertyType == typeof(DateOnly))
            {
                ParameterExpression param = Expression.Parameter(typeof(TEntity), "e");
                MemberExpression propAccess = Expression.Property(param, "StartDate");

                Expression predicate;

                ConstantExpression start = Expression.Constant(startDate);
                if (endDate.HasValue)
                {
                    ConstantExpression end = Expression.Constant(endDate.Value);
                    BinaryExpression greaterOrEqual = Expression.GreaterThanOrEqual(propAccess, start);
                    BinaryExpression lessOrEqual = Expression.LessThanOrEqual(propAccess, end);
                    predicate = Expression.AndAlso(greaterOrEqual, lessOrEqual);
                }
                else
                {
                    predicate = Expression.Equal(propAccess, start);
                }

                Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, param);
                queryable = queryable.Where(lambda);
            }
            else
            {
                throw new InvalidOperationException("La entidad no contiene una propiedad StartDate de tipo DateOnly.");
            }
        }

        // Total elementos
        int totalElements = await queryable.CountAsync();

        
        // Ordenamiento
        if (pagination.Ordenar == "StartDateTimeCombo")
        {
            queryable = queryable
                .OrderByDescending(e => EF.Property<DateOnly>(e, "StartDate"))
                .ThenByDescending(e => EF.Property<TimeOnly>(e, "StartTime"));
        }
        else
        {
            // Comportamiento estándar
            queryable = !string.IsNullOrEmpty(pagination.Ordenar)
                ? (pagination.Ascending
                    ? queryable.OrderBy(e => EF.Property<object>(e, pagination.Ordenar))
                    : queryable.OrderByDescending(e => EF.Property<object>(e, pagination.Ordenar)))
                : (pagination.Ascending
                    ? queryable.OrderBy(e => EF.Property<DateTime>(e, "CreatedAt"))
                    : queryable.OrderByDescending(e => EF.Property<DateTime>(e, "CreatedAt")));
        }

        // Paginado + mapeo
        List<TDto> resDto = await queryable
            .Paginar(pagination)
            .ProjectTo<TDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return PageableResponse.CreatePageableResponse(
            resDto,
            pagination.Pagina,
            pagination.RegistrosPorPagina,
            totalElements
        );
    }
    
    public async Task<UserEntity> GetUserByIdAsync(Guid? userId)
    {
        UserEntity? user = await _userManager.FindByIdAsync(userId!.ToString()!);
        return user ?? throw new NotFoundException("El usuario no fue encontrado");
    }
    
    private static bool IsNumericType(Type type)
    {
        return type == typeof(byte) || type == typeof(sbyte) ||
               type == typeof(short) || type == typeof(ushort) ||
               type == typeof(int) || type == typeof(uint) ||
               type == typeof(long) || type == typeof(ulong) ||
               type == typeof(float) || type == typeof(double) ||
               type == typeof(decimal);
    }
}
