using Bogus;
using Grains;
using Microsoft.Extensions.Hosting;
using TicketRush.Abstractions.Models;

namespace Simulations;

public class NewConcertSimulator : BackgroundService
{
    private readonly IClusterClient _clusterClient;

    public NewConcertSimulator(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var shopGrain = _clusterClient.GetGrain<IShopGrain>(Guid.Empty);
        int id = 0;

        for (int i = 0; i < 5; i++)
        {
            id++;
            decimal price = new Random().Next(10, 1000);
            int totalTickets = new Random().Next(100, 15000);
            var faker = new Faker<Concert>()
                .RuleFor(p => p.Id, f => id)
                .RuleFor(p => p.ConcertName, f => f.Name.FullName())
                .RuleFor(p => p.Price, f => price)
                .RuleFor(p => p.TotalTickets, f => totalTickets);

            try
            {
                var fakeConcert = faker.Generate();

                Console.WriteLine($"Creating concert {fakeConcert.ConcertName} - price {fakeConcert.Price} - number of tickets {fakeConcert.TotalTickets}");
                var concertGrain = _clusterClient.GetGrain<IConcertGrain>(fakeConcert.Id);
                await concertGrain.SaveConcert(fakeConcert);
            }
            catch
            {
                // just keep going
            }

            await Task.Delay(Random.Shared.Next(500, 2000), stoppingToken);
        }
    }
}