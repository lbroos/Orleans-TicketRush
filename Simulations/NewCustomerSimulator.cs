using Bogus;
using Grains;
using Microsoft.Extensions.Hosting;
using TicketRush.Abstractions.Models;

namespace Simulations;

public class NewCustomerSimulator : BackgroundService
{
    private readonly IClusterClient _clusterClient;

    public NewCustomerSimulator(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var customerId = Guid.NewGuid();
            var faker = new Faker<Customer>()
                .RuleFor(p => p.Id, f => customerId)
                .RuleFor(p => p.Name, f => f.Name.FullName())
                .RuleFor(p => p.Country, f => f.Address.Country())
                .RuleFor(p => p.City, f => $"{f.Address.City()}, {f.Address.State()}");

            try
            {
                var fakeCustomer = faker.Generate();

                Console.WriteLine($"Creating customer {fakeCustomer.Name} in {fakeCustomer.City} in {fakeCustomer.Country}");
                var userGrain = _clusterClient.GetGrain<IUserGrain>(fakeCustomer.Id);
                await userGrain.SaveCustomer(fakeCustomer);
                Console.WriteLine($"Created customer {fakeCustomer.Name} in {fakeCustomer.City} in {fakeCustomer.Country}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // just keep going
            }

            await Task.Delay(Random.Shared.Next(1, 100), stoppingToken);
        }
    }
}