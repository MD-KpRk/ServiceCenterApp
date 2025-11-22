using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServiceCenterApp.Models.Lookup;

namespace ServiceCenterApp.Models
{
    public class OrderStatusHistory
    {
        [Key]
        public int HistoryId { get; set; }

        public int OrderId { get; set; }

        public int OldStatusId { get; set; } // Старый статус
        public int NewStatusId { get; set; } // Новый статус

        public int EmployeeId { get; set; } // Кто изменил

        public DateTime ChangeDate { get; set; } = DateTime.Now;

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [ForeignKey("OldStatusId")]
        public virtual OrderStatus? OldStatus { get; set; }

        [ForeignKey("NewStatusId")]
        public virtual OrderStatus? NewStatus { get; set; }
    }
}