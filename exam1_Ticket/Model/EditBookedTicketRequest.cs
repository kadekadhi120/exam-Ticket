using System.ComponentModel.DataAnnotations;

namespace exam1_Ticket.Model
{
    public class EditBookedTicketRequest
    {
        //[Required]
        //public Guid BookedTicketId { get; set; }

        //[Required]
        //public List<TicketQuantityModel> Tickets { get; set; }
        public List<TicketRequest> Tickets { get; set; }

    }
}
