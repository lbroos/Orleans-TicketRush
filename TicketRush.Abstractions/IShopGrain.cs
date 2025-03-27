using TicketRush.Abstractions.Models;

namespace TicketRush.Abstractions;

public interface IShopGrain : IGrainWithGuidKey
{
    Task<Customer[]> GetCustomers();
    
    Task<Concert[]> GetConcerts();
}