namespace TicketRush.Abstractions;

public interface ITicketGrain: IGrainWithStringKey
{
    Task AssignToUser(Guid userId, decimal price);
    Task<Guid> GetOwner();
    Task<decimal> GetPrice();
}