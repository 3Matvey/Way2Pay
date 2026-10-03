var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Way2Pay_Orchestrator_Api>("way2pay-orchestrator-api");

builder.Build().Run();
