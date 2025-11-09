using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Windows.Controls;

namespace ServiceCenterApp.Models
{
    public class Device
    {
        [Key]
        public int DeviceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DeviceType { get; set; }

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; }

        [Required]
        [MaxLength(100)]
        public string Model { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        public bool WarrantyStatus { get; set; }

        // Навигационное свойство: одно устройство может быть во многих заказах
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}