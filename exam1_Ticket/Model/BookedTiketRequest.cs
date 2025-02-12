using System.ComponentModel.DataAnnotations;

public class BookedTiketRequest
{
    [Required]
    public string TicketCode { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity harus lebih dari 0")]
    public int Quantity { get; set; }

    
}
