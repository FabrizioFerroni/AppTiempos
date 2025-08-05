# ⏱️ AppTiempos - Mini CRM de Tiempos

Este proyecto es un mini CRM desarrollado con **.NET 8**, utilizando **Blazor** para el frontend y **ASP.NET Web API** para el backend. Su objetivo es ayudarte a **gestionar y controlar el tiempo dedicado a tareas** de forma simple y eficiente.

## 🛠️ Tecnologías utilizadas

- [.NET 8](https://dotnet.microsoft.com/)
- [Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [MySQL](https://www.mysql.com/)
- [Swagger](https://swagger.io/) (para testing del API)

## 📆 Estructura del proyecto

```
AppTiempos/
├── AppTiempos.Api                   # Web API (Backend)
├── AppTiempos.AppHost               # Parte para Aspire
├── AppTiempos.ServiceDefaults       # Aspire Functions
├── AppTiempos.SharedClasses         # Modelos compartidos entre cliente y servidor
├── AppTiempos.Web                   # Blazor WebAssembly (Frontend)
└── README.md
```

## 🚀 Funcionalidades

- Registro y autenticación de usuarios
- Crear, editar y eliminar tareas
- Controlar el tiempo dedicado a cada tarea
- Visualizar tiempos acumulados por tarea
- Conexión a base de datos MySQL
- API REST documentada con Swagger

## ⚙️ Configuración y ejecución

### 1. Clonar el repositorio

```bash
git clone https://github.com/FabrizioFerroni/AppTiempos.git
cd AppTiempos
```

### 2. Configurar la base de datos

Crear una base de datos MySQL (por ejemplo `minicrm_db`) y agregar la cadena de conexión en el archivo `appsettings.json` dentro de `AppTiempos.Api`:

```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=minicrm_db;user=root;password=tu_clave"
}
```

### 3. Ejecutar migraciones (si se usa EF Core)

```bash
cd AppTiempos.Api
dotnet ef database update
```

### 4. Levantar el proyecto

Para ejecutar toda la solución usando Aspire:

```bash
dotnet run --project AppTiempos.AppHost
```

Esto levantará tanto el frontend Blazor como el backend WebAPI y otros servicios configurados.

> Si solo querés ejecutar el backend:
>
> ```bash
> dotnet run --project AppTiempos.Api
> ```

> Si solo querés ejecutar el frontend:
>
> ```bash
> dotnet run --project AppTiempos.Web
> ```

## 📌 Notas

- Este proyecto está pensado como base para futuras ampliaciones, como reportes, exportaciones, integración con calendarios, etc.
- Se puede extender fácilmente para uso personal o para equipos chicos.

## 📄 Licencia

Este proyecto está bajo la licencia [MIT](LICENSE).
