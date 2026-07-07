namespace HelpDesk.Domain.Entities;

public class Priority : LookupEntity
{
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
