namespace HelpDesk.Domain.Entities;

public class Status : LookupEntity
{
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
