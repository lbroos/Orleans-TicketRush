using Microsoft.Extensions.Hosting;
using TicketRush.Abstractions.Models;

namespace Simulations;

public class SellingTicketsSimulator : BackgroundService
{
    private readonly IClusterClient _clusterClient;
    private Customer[] _customers = [];
    private Concert[] _concerts = [];

    public SellingTicketsSimulator(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                var shopGrain = _clusterClient.GetGrain<IShopGrain>(Guid.Empty);
                
                _customers = await shopGrain.GetCustomers();
                _concerts = await shopGrain.GetConcerts();

                if (_customers.Length > 0 && _concerts.Length > 0)
                {
                    await SellTicket();
                } 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // just keep going
            }
        }
    }

    private async Task SellTicket()
    {
        var randomConcert = _concerts[Random.Shared.Next(0, _concerts.Length)];
        var randomCustomer = _customers[Random.Shared.Next(0, _customers.Length)];
                    
        var userGrain = _clusterClient.GetGrain<IUserGrain>(randomCustomer.Id);
        var concertGrain = _clusterClient.GetGrain<IConcertGrain>(randomConcert.Id);

        var ticketPrice = await concertGrain.GetPrice();
        if (await userGrain.GetBalance() < ticketPrice)
        {
            var amount = Random.Shared.Next((int)ticketPrice, (int)ticketPrice * 2);
            await userGrain.AddFunds(amount);
            Console.WriteLine($"Added {amount} to the balance of customer {randomCustomer.Name}. New balance: {await userGrain.GetBalance()}");
        }
                    
        await userGrain.BuyTicket(randomConcert.Id);
        Console.WriteLine($"Customer {randomCustomer.Name} bought a ticket for {randomConcert.ConcertName}");
        
        await Task.Delay(100);
    }
}