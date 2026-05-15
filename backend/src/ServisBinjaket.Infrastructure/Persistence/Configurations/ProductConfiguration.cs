using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameSq).IsRequired().HasMaxLength(300);
        builder.Property(x => x.NameEn).HasMaxLength(300);
        builder.Property(x => x.SlugSq).IsRequired().HasMaxLength(300);
        builder.Property(x => x.SlugEn).HasMaxLength(300);
        builder.Property(x => x.ShortDescriptionSq).IsRequired();
        builder.Property(x => x.DescriptionSq).IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(10).HasDefaultValue("ALL");
        builder.Property(x => x.Condition).HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(x => x.SlugSq).IsUnique();
        builder.HasIndex(x => x.SlugEn).IsUnique();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
