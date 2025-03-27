using Microsoft.Extensions.Logging;
using TicketRush.Abstractions;
using TicketRush.Abstractions.Models;

namespace Grains;

public class ConcertGrain : Grain, IConcertGrain
{
    private readonly ILogger<UserGrain> _logger;
    private readonly IPersistentState<Concert> _concert;
    private readonly IPersistentState<List<string>> _soldTickets;
    private readonly IGrainFactory _grainFactory;

    public ConcertGrain(
        [PersistentState("concert", "grainState")] IPersistentState<Concert> concert,
        [PersistentState("soldTickets", "grainState")] IPersistentState<List<string>> soldTickets,
        IGrainFactory grainFactory,
        ILogger<UserGrain> logger)
    {
        _concert = concert;
        _soldTickets = soldTickets;
        _grainFactory = grainFactory;
        _logger = logger;
    }

    public async Task SaveConcert(Concert concert)
    {
        _concert.State = concert;
        _soldTickets.State = new();
        
        await _concert.WriteStateAsync();
        await _soldTickets.WriteStateAsync();
        
        var streamProvider = this.GetStreamProvider("shop");
        var newConcertStreamId = StreamId.Create("new_concert", Guid.Empty);
        var stream = streamProvider.GetStream<Concert>(newConcertStreamId);
        await stream.OnNextAsync(concert);
    }

    public Task<int> GetAvailableTickets() => Task.FromResult(_concert.State.TotalTickets - _soldTickets.State.Count);

    public Task<decimal> GetPrice() => Task.FromResult(_concert.State.Price);

    public async Task<string?> BuyTicket(Guid userId)
    {
        if (_soldTickets.State.Count >= _concert.State.TotalTickets) return null;

        var ticketId = $"{_concert.State.ConcertName}_Ticket_{_soldTickets.State.Count + 1}";
        _soldTickets.State.Add(ticketId);

        var ticketGrain = _grainFactory.GetGrain<ITicketGrain>(ticketId);
        await ticketGrain.AssignToUser(userId, await GetPrice());

        await _concert.WriteStateAsync();
        await _soldTickets.WriteStateAsync();
        
        return ticketId;
    }

    public Task<List<string>> GetAllSoldTickets() => Task.FromResult(_soldTickets.State);
}