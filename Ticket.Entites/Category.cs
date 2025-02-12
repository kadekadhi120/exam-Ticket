using System;
using System.Collections.Generic;

namespace Ticket.Entites;

public partial class Category
{
    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
