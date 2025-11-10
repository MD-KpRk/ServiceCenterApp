using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Windows.Controls;

namespace ServiceCenterApp.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required]
        [MaxLength(128)]
        public string? FullName { get; set; }

        [Required]
        [MaxLength(24)]
        public string? PhoneNumber { get; set; }

        [MaxLength(48)]
        public string? Email { get; set; }

        public string? Comment { get; set; }

        // Навигационное свойство: один клиент может иметь много заказов
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}