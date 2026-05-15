using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Infrastructure.Persistence.Configurations;

public class RepairRequestFileConfiguration : IEntityTypeConfiguration<RepairRequestFile>
{
    public void Configure(EntityTypeBuilder<RepairRequestFile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Path).IsRequired().HasMaxLength(500);
        builder.Property(x => x.OriginalName).IsRequired().HasMaxLength(300);
        builder.Property(x => x.MimeType).IsRequired().HasMaxLength(100);

        builder.HasOne(x => x.RepairRequest)
            .WithMany(x => x.Files)
            .HasForeignKey(x => x.RepairRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
