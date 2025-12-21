var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Gallery>("gallery");

builder.Build().Run();
