using System.ComponentModel.DataAnnotations;

namespace ServiceCenterApp.Models
{
    public class TransactionCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Name { get; set; }

        public bool IsExpense { get; set; } // Это категория расхода? (true=Расход, false=Приход)
    }
}