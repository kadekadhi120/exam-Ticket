using System.ComponentModel.DataAnnotations;

namespace exam1_Ticket.Model
{
    public class TicketQuantityModel
    {
        [Required]
        public string TicketCode { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity minimal 1.")]
        public int Quantity { get; set; }
    }
}
