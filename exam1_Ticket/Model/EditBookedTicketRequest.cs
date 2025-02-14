using System.ComponentModel.DataAnnotations;

namespace exam1_Ticket.Model
{
    public class EditBookedTicketRequest
    {
        
        public List<TicketRequest> Tickets { get; set; }

    }
}
