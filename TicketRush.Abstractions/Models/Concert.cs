namespace TicketRush.Abstractions.Models;

[GenerateSerializer]
public class Concert
{
    [Id(0)]
    public int Id { get; set; }
    
    [Id(1)]
    public string ConcertName { get; set; }

    [Id(2)]
    public int TotalTickets { get; set; }

    [Id(3)]
    public decimal Price{ get; set; }
}