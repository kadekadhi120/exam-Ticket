using System.ComponentModel.DataAnnotations;

namespace exam1_Ticket.Model
{
    public class CategoryModel
    {
        [Key]
        public Guid CategoryId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string CategoryName { get; set; }
    }
}
