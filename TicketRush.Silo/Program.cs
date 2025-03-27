using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.AddKeyedAzureTableClient("clustering");
builder.AddKeyedAzureBlobClient("grainState");
builder.AddKeyedAzureBlobClient("shop");

builder.UseOrleans(silo =>
{
    silo.Configure<ClusterMembershipOptions>(o =>
    {
        o.IAmAliveTablePublishTimeout = TimeSpan.FromSeconds(3);
        o.NumMissedTableIAmAliveLimit = 2;
    });
});

var app = builder.Build();
app.Run();