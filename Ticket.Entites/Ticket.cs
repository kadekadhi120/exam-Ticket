using System;
using System.Collections.Generic;

namespace Ticket.Entites;

public partial class Ticket
{
    public Guid TicketId { get; set; } = Guid.NewGuid();

    public string TicketCode { get; set; } = null!;

    public string TicketName { get; set; } = null!;

    public Guid CategoryId { get; set; }

    public DateOnly TanggalEvent { get; set; }

    public decimal Price { get; set; }

    public int Quota { get; set; }

    public virtual ICollection<BookedTiket> BookedTikets { get; set; } = new List<BookedTiket>();

    public virtual Category Category { get; set; } = null!;
}
