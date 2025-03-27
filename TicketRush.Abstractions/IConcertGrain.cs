using TicketRush.Abstractions.Models;

namespace TicketRush.Abstractions;

public interface IConcertGrain : IGrainWithIntegerKey
{
    Task SaveConcert(Concert concert);
    Task<int> GetAvailableTickets();
    Task<decimal> GetPrice();
    Task<string?> BuyTicket(Guid userId);
    Task<List<string>> GetAllSoldTickets();
}