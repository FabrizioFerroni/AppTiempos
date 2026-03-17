FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build


WORKDIR /src

RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && apt-get install -y nodejs

COPY ["AppTiemposV3.Web/AppTiemposV3.Web.csproj", "AppTiemposV3.Web/"]
COPY ["AppTiemposV3.SharedClases/AppTiemposV3.SharedClases.csproj", "AppTiemposV3.SharedClases/"]
COPY ["AppTiemposV3.ServiceDefaults/AppTiemposV3.ServiceDefaults.csproj", "AppTiemposV3.ServiceDefaults/"]

RUN dotnet restore "AppTiemposV3.Web/AppTiemposV3.Web.csproj"

COPY . .
WORKDIR "/src/AppTiemposV3.Web"

ARG PUBLIC_API_URL

RUN apt-get update && apt-get install -y dos2unix

RUN dos2unix Program.cs wwwroot/index.html && \
    sed -i "s|#API_URL#|${PUBLIC_API_URL}|g" Program.cs && \
    sed -i "s|#API_URL#|${PUBLIC_API_URL}|g" wwwroot/index.html 

RUN npm install

RUN npm run tailwind:build

RUN dotnet publish "AppTiemposV3.Web.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

COPY --from=build /app/publish/wwwroot .

COPY AppTiemposV3.Web/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]