using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Streams;
using TicketRush.Abstractions;
using TicketRush.Abstractions.Models;

namespace Grains;

[ImplicitStreamSubscription("shop")]
public class ShopGrain : Grain, IShopGrain
{
    private IPersistentState<List<Concert>> _concerts;
    private IPersistentState<List<Customer>> _customers;
    private readonly ILogger<ShopGrain> _logger;

    public ShopGrain(
        [PersistentState("concerts", "grainState")] IPersistentState<List<Concert>> concerts,
        [PersistentState("customers", "grainState")] IPersistentState<List<Customer>> customers,
        ILogger<ShopGrain> logger)
    {
        _concerts = concerts;
        _customers = customers;
        _logger = logger;
    }
    
    public override async Task OnActivateAsync(CancellationToken _)
    {
        var streamProvider = this.GetStreamProvider("shop");
        var newCustomerStreamId = StreamId.Create("new_customer", this.GetPrimaryKey());
        var customerStream = streamProvider.GetStream<Customer>(newCustomerStreamId);

        await customerStream.SubscribeAsync(
            async (data, token) =>
            {
                Console.WriteLine($"New customer added to shopGrain {this.GetPrimaryKey()}");
                await UpdateCustomers(data);
            });
        
        var newConcertStreamId = StreamId.Create("new_concert", this.GetPrimaryKey());
        var concertStream = streamProvider.GetStream<Concert>(newConcertStreamId);

        await concertStream.SubscribeAsync(
            async (data, token) =>
            {
                await UpdateConcerts(data);
            });
    }
    
    public async Task<Customer[]> GetCustomers()
    {
        await _customers.ReadStateAsync();
        return _customers.State.ToArray();
    }

    public async Task UpdateCustomers(Customer customer)
    {
        if (_customers.State.All(x => x.Id != customer.Id))
        {
            _customers.State.Add(customer);
            await _customers.WriteStateAsync();
        }
    }

    public async Task<Concert[]> GetConcerts()
    {
        await _concerts.ReadStateAsync();
        return _concerts.State.ToArray();
    }

    public async Task UpdateConcerts(Concert concert)
    {
        if (_concerts.State.All(x => x.Id != concert.Id))
        {
            _concerts.State.Add(concert);
            await _concerts.WriteStateAsync();
        }
    }
}