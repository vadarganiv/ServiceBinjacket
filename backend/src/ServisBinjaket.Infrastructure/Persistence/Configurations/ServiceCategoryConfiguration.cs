using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Infrastructure.Persistence.Configurations;

public class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameSq).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.SlugSq).IsRequired().HasMaxLength(200);
        builder.Property(x => x.SlugEn).HasMaxLength(200);

        builder.HasIndex(x => x.SlugSq).IsUnique();
        builder.HasIndex(x => x.SlugEn).IsUnique();

        builder.HasData(
            new ServiceCategory { Id = 1, NameSq = "Smartphone", NameEn = "Smartphones", SlugSq = "smartphone", SlugEn = "smartphones", SortOrder = 1, IsPublished = true },
            new ServiceCategory { Id = 2, NameSq = "Laptopë", NameEn = "Laptops", SlugSq = "laptope-sherbim", SlugEn = "laptops-service", SortOrder = 2, IsPublished = true },
            new ServiceCategory { Id = 3, NameSq = "Tablet", NameEn = "Tablets", SlugSq = "tablet", SlugEn = "tablets", SortOrder = 3, IsPublished = true },
            new ServiceCategory { Id = 4, NameSq = "Të tjera", NameEn = "Other", SlugSq = "te-tjera", SlugEn = "other", SortOrder = 4, IsPublished = true }
        );
    }
}
