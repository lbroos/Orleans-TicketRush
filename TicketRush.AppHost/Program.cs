var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage").RunAsEmulator();

var clustering = storage.AddTables("clustering");
var grainState = storage.AddBlobs("grainState");
var memoryStore = storage.AddBlobs("shop");

var orleans = builder.AddOrleans("orleans")
    .WithClusterId("TicketRushCluster")
    .WithServiceId("TicketRushService")
    .WithClustering(clustering)
    .WithGrainStorage(grainState)
    .WithGrainStorage(memoryStore)
    .WithMemoryStreaming("shop");

var ticketRushSilo = builder.AddProject<Projects.TicketRush_Silo>("ticketRushSilo")
    .WithReference(orleans)
    .WaitFor(storage)
    .WaitFor(clustering)
    .WaitFor(grainState)
    .WaitFor(memoryStore)
    .WithReplicas(5);

var ticketRushWeb = builder.AddProject<Projects.TicketRush_Web>("ticketRush")
    .WithReference(orleans.AsClient())
    .WaitFor(ticketRushSilo);

builder.AddProject<Projects.Simulations>("simulations")
    .WithReference(orleans.AsClient())
    .WaitFor(ticketRushSilo);

builder.Build().Run();
