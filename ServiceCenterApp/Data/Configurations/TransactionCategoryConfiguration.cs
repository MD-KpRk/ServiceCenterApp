using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;

namespace ServiceCenterApp.Data.Configurations
{
    public class TransactionCategoryConfiguration : IEntityTypeConfiguration<TransactionCategory>
    {
        public void Configure(EntityTypeBuilder<TransactionCategory> builder)
        {
            builder.HasData(
                new TransactionCategory { CategoryId = 1, Name = "Оплата заказа", IsExpense = false },
                new TransactionCategory { CategoryId = 2, Name = "Прочий доход", IsExpense = false },

                new TransactionCategory { CategoryId = 3, Name = "Закупка запчастей", IsExpense = true },
                new TransactionCategory { CategoryId = 4, Name = "Аренда", IsExpense = true },
                new TransactionCategory { CategoryId = 5, Name = "Реклама", IsExpense = true },
                new TransactionCategory { CategoryId = 6, Name = "Налоги", IsExpense = true },
                new TransactionCategory { CategoryId = 7, Name = "Хоз. нужды", IsExpense = true },
                new TransactionCategory { CategoryId = 8, Name = "Прочее", IsExpense = true }
            );
        }
    }
}
