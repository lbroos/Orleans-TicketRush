using Microsoft.Extensions.Logging;
using TicketRush.Abstractions;
using TicketRush.Abstractions.Models;

namespace Grains;

public class UserGrain: Grain, IUserGrain
{
    private readonly IPersistentState<Customer> _customer;
    private readonly IPersistentState<List<string>> _ownedTickets;
    private readonly IPersistentState<decimal> _balance;
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<UserGrain> _logger;

    public UserGrain(
        [PersistentState("customer", "grainState")] IPersistentState<Customer> customer,
        [PersistentState("ownedTickets", "grainState")] IPersistentState<List<string>> ownedTickets,
        [PersistentState("balance", "grainState")] IPersistentState<decimal> balance,
        IGrainFactory grainFactory,
        ILogger<UserGrain> logger)
    {
        _customer = customer;
        _ownedTickets = ownedTickets;
        _balance = balance;
        _grainFactory = grainFactory;
        _logger = logger;
    }

    public async Task SaveCustomer(Customer customer)
    {
        _customer.State = customer;
        _balance.State = 1000m; // Initial balance
        _ownedTickets.State = new();
        
        await _customer.WriteStateAsync();
        await _balance.WriteStateAsync();
        await _ownedTickets.WriteStateAsync();
        
        var streamProvider = this.GetStreamProvider("shop");
        var newCustomerStreamId = StreamId.Create("new_customer", Guid.Empty);
        var stream = streamProvider.GetStream<Customer>(newCustomerStreamId);
        await stream.OnNextAsync(customer);
    }

    public Task<Customer> GetCustomer()
    {
        return Task.FromResult(_customer.State);
    }

    public Task<decimal> GetBalance() => Task.FromResult(_balance.State);
    public Task AddFunds(decimal amount) { _balance.State += amount; return Task.CompletedTask; }

    public async Task<bool> BuyTicket(int concertId)
    {
        var concertGrain = _grainFactory.GetGrain<IConcertGrain>(concertId);
        decimal price = await concertGrain.GetPrice();

        if (_balance.State < price) return false;

        var ticketId = await concertGrain.BuyTicket(this.GetPrimaryKey());
        if (ticketId == null) return false;

        _balance.State -= price;
        _ownedTickets.State.Add(ticketId);
        
        await _balance.WriteStateAsync();
        await _ownedTickets.WriteStateAsync();
        
        return true;
    }

    public Task<List<string>> GetOwnedTickets() => Task.FromResult(_ownedTickets.State);
}