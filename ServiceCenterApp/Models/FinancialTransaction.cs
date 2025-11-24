using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenterApp.Models
{
    public enum TransactionType
    {
        Income = 1,
        Expense = 2
    }

    public enum PaymentMethod
    {
        Cash = 1,
        Card = 2,
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

        // Ссылка на заказ
        public int? RelatedOrderId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual TransactionCategory? Category { get; set; }

        [ForeignKey("RelatedOrderId")]
        public virtual Order? RelatedOrder { get; set; }
    }
}