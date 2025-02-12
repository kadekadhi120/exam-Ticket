using System;
using System.Collections.Generic;

namespace Ticket.Entites;

public partial class BookedTiket
{
    public Guid BookedTicketId { get; set; }

    public Guid TicketId { get; set; }

    public int Quantity { get; set; }

    public DateTime TanggalBooking { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
