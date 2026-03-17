IDistributedApplicationBuilder? builder = DistributedApplication.CreateBuilder(args);

/*builder.AddProject<Projects.AppTiemposV3_Api>("api");


builder.AddProject<Projects.AppTiemposV3_Web>("frontend");*/

IResourceBuilder<ProjectResource>? api = builder.AddProject<Projects.AppTiemposV3_Api>("api");

// Registras la Web (Blazor) y le pasas la referencia de la API
builder.AddProject<Projects.AppTiemposV3_Web>("frontend")
       .WithReference(api)
       .WithExternalHttpEndpoints();


builder.Build().Run();