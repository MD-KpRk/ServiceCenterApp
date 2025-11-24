using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;

namespace ServiceCenterApp.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(o => o.RegistrationDate).HasDefaultValueSql("GETDATE()");

            // Настраиваем связь для СОЗДАТЕЛЯ заказа (Creator)
            builder.HasOne(order => order.CreatorEmployee) // У одного Заказа есть один Создатель
                   .WithMany(employee => employee.CreatedOrders) // У одного Сотрудника есть много СОЗДАННЫХ им заказов
                   .HasForeignKey(order => order.CreatorEmployeeId) // Внешний ключ в таблице Orders
                   .OnDelete(DeleteBehavior.Restrict); // Запретить удаление сотрудника, если он создал заказы

            // Настраиваем связь для ИСПОЛНИТЕЛЯ заказа (Acceptor)
            builder.HasOne(order => order.AcceptorEmployee) // У одного Заказа есть один Исполнитель
                   .WithMany(employee => employee.AcceptedOrders) // У одного Сотрудника есть много ПРИНЯТЫХ им заказов
                   .HasForeignKey(order => order.AcceptorEmployeeId) // Внешний ключ в таблице Orders
                   .OnDelete(DeleteBehavior.Restrict); // Запретить удаление сотрудника, если он исполняет заказы

            // Настраиваем связь с Клиентом
            builder.HasOne(o => o.Client)
                   .WithMany(c => c.Orders)
                   .HasForeignKey(o => o.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}