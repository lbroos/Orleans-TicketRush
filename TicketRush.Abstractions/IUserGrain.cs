using TicketRush.Abstractions.Models;

namespace TicketRush.Abstractions;

public interface IUserGrain : IGrainWithGuidKey
{
    Task SaveCustomer(Customer customer);
    
    Task<Customer> GetCustomer();
    
    Task<decimal> GetBalance();
    
    Task AddFunds(decimal amount);
    
    Task<bool> BuyTicket(int concertId);
    
    Task<List<string>> GetOwnedTickets();
}