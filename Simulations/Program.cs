using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Simulations;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddKeyedAzureTableClient("clustering");
builder.AddKeyedAzureBlobClient("grainState");
builder.AddKeyedAzureBlobClient("shop");

builder.UseOrleansClient();

builder.Services.AddHostedService<NewConcertSimulator>();
builder.Services.AddHostedService<NewCustomerSimulator>();
builder.Services.AddHostedService<SellingTicketsSimulator>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.Run();