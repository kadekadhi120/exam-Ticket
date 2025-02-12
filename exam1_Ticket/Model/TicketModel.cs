using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ticket.Entites;

namespace exam1_Ticket.Model
{
    public class TicketModel
    {
        public DateTime TanggalEvent { get; set; } // Sesuaikan tipe dengan database
        public int Quota { get; set; }

        public string TicketCode { get; set; }
        public string TicketName { get; set; }

        public int Quantity { get; set; }
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }

        // Foreign Key untuk kategori
        public int CategoryId { get; set; }
        public Category Category { get; set; }



    }
}
