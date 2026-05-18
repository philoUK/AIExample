var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var administrationEventStore = postgres.AddDatabase("administration-eventstore");

builder.AddProject<Projects.Api>("api")
    .WithReference(administrationEventStore)
    .WaitFor(administrationEventStore);

builder.Build().Run();
