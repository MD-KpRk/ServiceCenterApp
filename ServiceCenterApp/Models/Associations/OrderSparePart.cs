using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenterApp.Models.Associations
{
    public class OrderSparePart
    {
        public int OrderId { get; set; }
        public int PartId { get; set; }
        public int Quantity { get; set; }

        // Цена, по которой мы ПРОДАЛИ клиенту в этом конкретном заказе
        [Column(TypeName = "decimal(10, 2)")]
        public decimal SalePrice { get; set; }

        // Себестоимость на момент продажи (для отчетов по прибыли)
        [Column(TypeName = "decimal(10, 2)")]
        public decimal CostPrice { get; set; }

        // Навигационные свойства
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("PartId")]
        public virtual SparePart? SparePart { get; set; }
    }
}