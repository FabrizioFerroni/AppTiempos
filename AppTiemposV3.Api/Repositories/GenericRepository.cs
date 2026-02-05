using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.Api.Helpers;
using AppTiemposV3.Api.Utilidades;
using AppTiemposV3.SharedClases.Annotations;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.Exceptions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using static AppTiemposV3.Api.Helpers.EntityNavigationHelper;
using static AppTiemposV3.Api.Helpers.QueryableExtensions;

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

        bool hasNavigations = HasNavigations<TEntity>();

        List<TDto> resDto;

        if (hasNavigations)
        {
            List<TEntity>? list = await queryable
                .IncludeAllNavigations<TEntity>() // si lo tenés
                .Paginar(pagination)
                .ToListAsync();

            resDto = _mapper.Map<List<TDto>>(list);
        }
        else
        {
            resDto = await queryable
                .Paginar(pagination)
                .ProjectTo<TDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        // Paginado + mapeo
        /*List<TDto> resDto = await queryable
            .Paginar(pagination)
            .ProjectTo<TDto>(_mapper.ConfigurationProvider)
            .ToListAsync();*/

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

    public async Task<Pageable<List<TDto>>> GetAllPaginatedFaAsync<TEntity, TDto>(PaginationDtoAdvanced pagination, Guid? userId) where TEntity : class
    {
        IQueryable<TEntity> queryable = _dbContext.Set<TEntity>().AsQueryable();
        
        // Filtro por UserId si aplica
        if (userId is not null && userId != Guid.Empty)
        {
            UserEntity? user = await GetUserByIdAsync(userId);
            
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
        
        
        // Aplicar filtros avanzados dinámicamente
        if (pagination.Filters is not null && pagination.Filters.Any())
        {
            ParameterExpression param = Expression.Parameter(typeof(TEntity), "e");
            Expression? combined = null;

            foreach (AdvancedFilters filter in pagination.Filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Key) || string.IsNullOrWhiteSpace(filter.Value))
                    continue;
                
                PropertyInfo? prop = typeof(TEntity).GetProperty(filter.Key);
                if (prop == null) continue;
                
                MemberExpression property = Expression.Property(param, prop);
                ConstantExpression valueConst;
                
                Expression condition;
                
                if (prop.PropertyType == typeof(string))
                {
                    MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                    valueConst = Expression.Constant(filter.Value);
                    condition = Expression.Call(property, containsMethod, valueConst);
                }
                else if (prop.PropertyType == typeof(Guid))
                {
                    if (Guid.TryParse(filter.Value, out Guid guidValue))
                    {
                        valueConst = Expression.Constant(guidValue);
                        condition = Expression.Equal(property, valueConst);
                    }
                    else continue;
                }
                else if (IsNumericType(prop.PropertyType))
                {
                    if (decimal.TryParse(filter.Value, out decimal numValue))
                    {
                        valueConst = Expression.Constant(Convert.ChangeType(numValue, prop.PropertyType));
                        condition = Expression.Equal(property, valueConst);
                    }
                    else continue;
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    if (bool.TryParse(filter.Value, out bool boolVal))
                    {
                        valueConst = Expression.Constant(boolVal);
                        condition = Expression.Equal(property, valueConst);
                    }
                    else continue;
                }
                else
                {
                    // fallback: ToString().Contains()
                    MethodInfo toStringMethod = prop.PropertyType.GetMethod("ToString", Type.EmptyTypes)!;
                    MethodCallExpression toStringCall = Expression.Call(property, toStringMethod);
                    MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                    valueConst = Expression.Constant(filter.Value);
                    condition = Expression.Call(toStringCall, containsMethod, valueConst);
                }
                
                combined = combined == null ? condition : Expression.AndAlso(combined, condition);
            }
            
            if (combined != null)
            {
                Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(combined, param);
                queryable = queryable.Where(lambda);
            }
        }
        else {
            List<AdvancedFilters> filtersNull = new List<AdvancedFilters>();
            pagination.Filters = filtersNull.ToArray();
        }

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

        //bool hasCollections = typeof(TEntity)
        //            .GetProperties()
        //            .Any(p =>
        //                p.PropertyType.IsGenericType &&
        //                p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
        //            );

        bool hasNavigations = HasNavigations<TEntity>();

        List<TDto> resDto;

        /*if (hasCollections)
        {
            queryable = ApplyIncludes(queryable);

            List<TEntity>? entities = await queryable
                .PaginarAdvanced(pagination)
                .ToListAsync();

            resDto = _mapper.Map<List<TDto>>(entities);
        }
        else
        {
            resDto = await queryable
                .PaginarAdvanced(pagination)
                .ProjectTo<TDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }*/

        if (hasNavigations)
        {
            List<TEntity>? list = await queryable
                .IncludeAllNavigations<TEntity>() // si lo tenés
                .PaginarAdvanced(pagination)
                .ToListAsync();

            resDto = _mapper.Map<List<TDto>>(list);
        }
        else
        {
            resDto = await queryable
                .PaginarAdvanced(pagination)
                .ProjectTo<TDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        //List<TDto> resDto = await queryable
        //    .PaginarAdvanced(pagination)
        //    .ProjectTo<TDto>(_mapper.ConfigurationProvider)
        //    .ToListAsync();

        return PageableResponse.CreatePageableResponse(
            resDto,
            pagination.Pagina,
            pagination.RegistrosPorPagina,
            totalElements
        );
    }

    public async Task<Pageable<List<TDto>>> GetAllPaginatedAuditAsync<TEntity, TDto>(PaginationDtoAdvanced pagination) where TEntity : class
    {
        IQueryable<TEntity> queryable = _dbContext.Set<TEntity>().AsQueryable();
        
        if (pagination.Filters is not null && pagination.Filters.Any())
        {
            ParameterExpression param = Expression.Parameter(typeof(TEntity), "e");
            Expression? combined = null;
            DateTime? startDate = null;
            DateTime? endDate = null;

            foreach (AdvancedFilters filter in pagination.Filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Key) || string.IsNullOrWhiteSpace(filter.Value))
                    continue;
                
                if (filter.Key == "StartDate")
                {
                    if (DateTime.TryParse(filter.Value, out DateTime dt))
                        startDate = dt.Date;
                    continue;
                }

                if (filter.Key == "EndDate")
                {
                    if (DateTime.TryParse(filter.Value, out DateTime dt))
                        endDate = dt.Date.AddDays(1).AddTicks(-1);
                    continue;
                }
                
                PropertyInfo? prop = typeof(TEntity).GetProperty(filter.Key);
                if (prop == null) continue;
                
                MemberExpression property = Expression.Property(param, prop);
                ConstantExpression valueConst;
                
                Expression condition;
                
                
                
                if (prop.PropertyType == typeof(string))
                {
                    MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                    valueConst = Expression.Constant(filter.Value);
                    condition = Expression.Call(property, containsMethod, valueConst);
                }
                else if (prop.PropertyType == typeof(Guid))
                {
                    if (Guid.TryParse(filter.Value, out Guid guidValue))
                    {
                        valueConst = Expression.Constant(guidValue);
                        condition = Expression.Equal(property, valueConst);
                    }
                    else continue;
                }
                else if (IsNumericType(prop.PropertyType))
                {
                    if (decimal.TryParse(filter.Value, out decimal numValue))
                    {
                        valueConst = Expression.Constant(Convert.ChangeType(numValue, prop.PropertyType));
                        condition = Expression.Equal(property, valueConst);
                    }
                    else continue;
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    if (bool.TryParse(filter.Value, out bool boolVal))
                    {
                        valueConst = Expression.Constant(boolVal);
                        condition = Expression.Equal(property, valueConst);
                    }
                    else continue;
                }
                else
                {
                    // fallback: ToString().Contains()
                    MethodInfo toStringMethod = prop.PropertyType.GetMethod("ToString", Type.EmptyTypes)!;
                    MethodCallExpression toStringCall = Expression.Call(property, toStringMethod);
                    MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                    valueConst = Expression.Constant(filter.Value);
                    condition = Expression.Call(toStringCall, containsMethod, valueConst);
                }
                
                combined = combined == null ? condition : Expression.AndAlso(combined, condition);
            }
            
            if (startDate.HasValue || endDate.HasValue)
            {
                PropertyInfo? createdAtProp = typeof(TEntity).GetProperty("CreatedAt");
                if (createdAtProp != null)
                {
                    MemberExpression createdAt = Expression.Property(param, createdAtProp);

                    if (startDate.HasValue && endDate.HasValue)
                    {
                        // BETWEEN
                        BinaryExpression? gte = Expression.GreaterThanOrEqual(
                            createdAt,
                            Expression.Constant(startDate.Value)
                        );

                        BinaryExpression? lt = Expression.LessThan(
                            createdAt,
                            Expression.Constant(endDate.Value)
                        );

                        BinaryExpression? between = Expression.AndAlso(gte, lt);
                        combined = combined == null ? between : Expression.AndAlso(combined, between);
                    }
                    else if (startDate.HasValue)
                    {
                        BinaryExpression? gte = Expression.GreaterThanOrEqual(
                            createdAt,
                            Expression.Constant(startDate.Value)
                        );
                        combined = combined == null ? gte : Expression.AndAlso(combined, gte);
                    }
                    else if (endDate.HasValue)
                    {
                        BinaryExpression? lt = Expression.LessThan(
                            createdAt,
                            Expression.Constant(endDate.Value)
                        );
                        combined = combined == null ? lt : Expression.AndAlso(combined, lt);
                    }
                }
            }
            
            
            if (combined != null)
            {
                Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(combined, param);
                queryable = queryable.Where(lambda);
            }
        }
        
        int totalElements = await queryable.CountAsync();
        
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
        
        List<TEntity> entities = await queryable
            .Include("User")
            .PaginarAdvanced(pagination)
            .ToListAsync();

        List<TDto> resDto = _mapper.Map<List<TDto>>(entities);
        
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

    private IQueryable<TEntity> ApplyIncludes<TEntity>(IQueryable<TEntity> query)
    where TEntity : class
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(TEntity));
        if (entityType == null) return query;

        foreach (var navigation in entityType.GetNavigations())
        {
            query = query.Include(navigation.Name);
        }

        return query;
    }

    public async Task<Pageable<List<TEntity>>> GetAllPaginatedReportedAsync<TEntity>(PaginationDto pagination, Guid? userId) where TEntity : class
    {
        ParameterExpression paramBuscar = Expression.Parameter(typeof(TEntity), "e");
        MemberExpression propertyBuscar = Expression.Property(paramBuscar, "Name");
        Expression<Func<TEntity, object>> expressionBuscar = Expression.Lambda<Func<TEntity, object>>(
            Expression.Convert(propertyBuscar, typeof(object)), paramBuscar
        );

        IQueryable<TEntity> queryable = _dbContext.Set<TEntity>().AsQueryable();

        if (userId is not null && userId != Guid.Empty)
        {
            UserEntity user = await GetUserByIdAsync(userId);

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
                
            MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
            ConstantExpression searchValue = Expression.Constant(pagination.Search);
            MethodCallExpression containsExpression = Expression.Call(propertyAccess, containsMethod, searchValue);
            lambda = Expression.Lambda<Func<TEntity, bool>>(containsExpression, param);
           

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

        // Paginación
        List<TEntity> resDto = await queryable
            .Paginar(pagination)
            .ToListAsync();

        Pageable<List<TEntity>> response = PageableResponse.CreatePageableResponse(
            resDto,
            pagination.Pagina,
            pagination.RegistrosPorPagina,
            totalElements
        );

        return response;
    }
}
