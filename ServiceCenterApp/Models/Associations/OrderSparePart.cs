using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenterApp.Models.Associations
{
    public class OrderSparePart
    {
        public int OrderId { get; set; }
        public int PartId { get; set; }
        public int Quantity { get; set; }

        // Навигационные свойства
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("PartId")]
        public virtual SparePart SparePart { get; set; }
    }
}