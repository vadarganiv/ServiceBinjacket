using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Infrastructure.Persistence.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameSq).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.SlugSq).IsRequired().HasMaxLength(200);
        builder.Property(x => x.SlugEn).HasMaxLength(200);

        builder.HasIndex(x => x.SlugSq).IsUnique();
        builder.HasIndex(x => x.SlugEn).IsUnique();

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new ProductCategory { Id = 1, NameSq = "Telefona", NameEn = "Phones", SlugSq = "telefona", SlugEn = "phones", SortOrder = 1, IsPublished = true },
            new ProductCategory { Id = 2, NameSq = "Laptopë", NameEn = "Laptops", SlugSq = "laptope", SlugEn = "laptops", SortOrder = 2, IsPublished = true },
            new ProductCategory { Id = 3, NameSq = "Aksesorë", NameEn = "Accessories", SlugSq = "aksesore", SlugEn = "accessories", SortOrder = 3, IsPublished = true },
            new ProductCategory { Id = 4, NameSq = "TV & Audio", NameEn = "TV & Audio", SlugSq = "tv-audio", SlugEn = "tv-audio", SortOrder = 4, IsPublished = true }
        );
    }
}
