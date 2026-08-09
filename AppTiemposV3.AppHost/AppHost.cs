var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AppTiemposV3_Api>("api");


builder.AddProject<Projects.AppTiemposV3_Web>("frontend");


builder.Build().Run();