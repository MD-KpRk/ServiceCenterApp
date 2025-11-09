using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models.Lookup;

namespace ServiceCenterApp.Data.Configurations
{
    public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
    {
        public void Configure(EntityTypeBuilder<DocumentType> builder)
        {
            builder.HasIndex(dt => dt.TypeName).IsUnique();

            builder.HasData(
                new DocumentType { DocumentTypeId = 1, TypeName = "Акт приема-передачи" },
                new DocumentType { DocumentTypeId = 2, TypeName = "Акт выполненных работ" },
                new DocumentType { DocumentTypeId = 3, TypeName = "Гарантийный талон" }
            );
        }
    }
}