namespace HelpDesk.Domain.Entities;

public class Category : LookupEntity
{
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
