FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

USER root
RUN mkdir -p /app/keys /app/wwwroot /app/Files && \
    chown -R app:app /app/keys /app/wwwroot /app/Files && \
    chmod -R 755 /app/keys /app/wwwroot /app/Files
USER app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["AppTiemposV3.Api/AppTiemposV3.Api.csproj", "AppTiemposV3.Api/"]
COPY ["AppTiemposV3.SharedClases/AppTiemposV3.SharedClases.csproj", "AppTiemposV3.SharedClases/"]
COPY ["AppTiemposV3.Console/AppTiemposV3.Console.csproj", "AppTiemposV3.Console/"]
COPY ["AppTiemposV3.ServiceDefaults/AppTiemposV3.ServiceDefaults.csproj", "AppTiemposV3.ServiceDefaults/"]

RUN dotnet restore "AppTiemposV3.Api/AppTiemposV3.Api.csproj"
RUN dotnet restore "AppTiemposV3.Console/AppTiemposV3.Console.csproj"

COPY . .

WORKDIR "/src/AppTiemposV3.Api"
RUN dotnet build "AppTiemposV3.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AppTiemposV3.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

COPY --from=publish /app/publish .

COPY --from=build /src/AppTiemposV3.Api/Files/colors.json ./Files/ 

ENTRYPOINT ["dotnet", "AppTiemposV3.Api.dll"]