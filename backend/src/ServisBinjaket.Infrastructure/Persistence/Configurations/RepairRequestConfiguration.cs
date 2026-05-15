using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Infrastructure.Persistence.Configurations;

public class RepairRequestConfiguration : IEntityTypeConfiguration<RepairRequest>
{
    public void Configure(EntityTypeBuilder<RepairRequest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DeviceType).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Brand).HasMaxLength(200);
        builder.Property(x => x.Model).HasMaxLength(200);
        builder.Property(x => x.ProblemDescription).IsRequired();
        builder.Property(x => x.PreferredDeliveryMethod).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.RepairRequests)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Service)
            .WithMany(x => x.RepairRequests)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
