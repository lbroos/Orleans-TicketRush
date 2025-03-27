using Microsoft.Extensions.Logging;
using TicketRush.Abstractions;

namespace Grains;

public class TicketGrain : Grain, ITicketGrain
{
    private readonly ILogger<TicketGrain> _logger;
    private readonly IPersistentState<Guid> _ownerUuid;
    private readonly IPersistentState<decimal> _price;
    
    public TicketGrain(
        [PersistentState("owner", "grainState")] IPersistentState<Guid> ownerUuid,
        [PersistentState("price", "grainState")] IPersistentState<decimal> price,
        ILogger<TicketGrain> logger)
    {
        _ownerUuid = ownerUuid;
        _price = price;
        _logger = logger;
    }
    
    public async Task AssignToUser(Guid userId, decimal price)
    {
        _ownerUuid.State = userId;
        _price.State = price;
        
        await _ownerUuid.WriteStateAsync();
        await _price.WriteStateAsync();
    }

    public Task<Guid> GetOwner() => Task.FromResult(_ownerUuid.State);
    public Task<decimal> GetPrice() => Task.FromResult(_price.State);
}