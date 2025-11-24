using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenterApp.Models
{
    public enum TransactionType
    {
        Income = 1, // Приход
        Expense = 2 // Расход
    }

    public enum PaymentMethod
    {
        Cash = 1,   // Наличные
        Card = 2,   // Карта/Безнал
    }

    public class FinancialTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; } 

        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Description { get; set; } 

        public int CategoryId { get; set; }

        // Ссылки (опционально, чтобы понимать откуда ноги растут)
        public int? RelatedOrderId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual TransactionCategory? Category { get; set; }
    }
}