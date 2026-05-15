using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameSq).IsRequired().HasMaxLength(300);
        builder.Property(x => x.NameEn).HasMaxLength(300);
        builder.Property(x => x.SlugSq).IsRequired().HasMaxLength(300);
        builder.Property(x => x.SlugEn).HasMaxLength(300);
        builder.Property(x => x.ShortDescriptionSq).IsRequired();
        builder.Property(x => x.DescriptionSq).IsRequired();
        builder.Property(x => x.PriceNoteSq).IsRequired().HasMaxLength(500);
        builder.Property(x => x.PriceNoteEn).HasMaxLength(500);

        builder.HasIndex(x => x.SlugSq).IsUnique();
        builder.HasIndex(x => x.SlugEn).IsUnique();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Services)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Service
            {
                Id = 1, CategoryId = 1,
                NameSq = "Ndërrimi i ekranit", NameEn = "Screen replacement",
                SlugSq = "nderrimi-i-ekranit", SlugEn = "screen-replacement",
                ShortDescriptionSq = "Ndërrojmë ekranin e telefonit tuaj me cilësi të lartë.",
                ShortDescriptionEn = "We replace your phone screen with high quality parts.",
                DescriptionSq = "Shërbim profesional i ndërrimit të ekranit për të gjitha modelet e smartfonëve.",
                DescriptionEn = "Professional screen replacement service for all smartphone models.",
                PriceNoteSq = "Çmimi varet nga modeli", PriceNoteEn = "Price depends on model",
                IsPublished = true
            },
            new Service
            {
                Id = 2, CategoryId = 1,
                NameSq = "Ndërrimi i baterisë", NameEn = "Battery replacement",
                SlugSq = "nderrimi-i-baterise", SlugEn = "battery-replacement",
                ShortDescriptionSq = "Ndërrojmë baterinë e vjetër me të re.",
                ShortDescriptionEn = "We replace your old battery with a new one.",
                DescriptionSq = "Bateritë tona janë origjinale ose me cilësi të lartë për çdo model.",
                DescriptionEn = "Our batteries are original or high quality for every model.",
                PriceNoteSq = "Çmimi varet nga modeli", PriceNoteEn = "Price depends on model",
                IsPublished = true
            },
            new Service
            {
                Id = 3, CategoryId = 1,
                NameSq = "Riparim saldimi", NameEn = "Soldering repair",
                SlugSq = "riparim-saldimi", SlugEn = "soldering-repair",
                ShortDescriptionSq = "Riparim i komponentëve elektronike me saldim.",
                ShortDescriptionEn = "Electronic component repair with soldering.",
                DescriptionSq = "Riparim profesional i qarkut elektronik, lidhjet e dobëta, komponentët e dëmtuar.",
                DescriptionEn = "Professional electronic circuit repair, weak connections, damaged components.",
                PriceNoteSq = "Çmimi pas diagnostikimit", PriceNoteEn = "Price after diagnostics",
                IsPublished = true
            },
            new Service
            {
                Id = 4, CategoryId = 2,
                NameSq = "Riparim laptopi", NameEn = "Laptop repair",
                SlugSq = "riparim-laptopi", SlugEn = "laptop-repair",
                ShortDescriptionSq = "Riparim i plotë i laptopëve të çdo marke.",
                ShortDescriptionEn = "Full repair for laptops of any brand.",
                DescriptionSq = "Ekran, tastierë, saldim, softuer — riparojmë çdo problem të laptopit tuaj.",
                DescriptionEn = "Screen, keyboard, soldering, software — we fix any laptop problem.",
                PriceNoteSq = "Çmimi pas diagnostikimit", PriceNoteEn = "Price after diagnostics",
                IsPublished = true
            },
            new Service
            {
                Id = 5, CategoryId = 1,
                NameSq = "Diagnostikim", NameEn = "Diagnostics",
                SlugSq = "diagnostikim", SlugEn = "diagnostics",
                ShortDescriptionSq = "Diagnostikim falas i problemit të pajisjes suaj.",
                ShortDescriptionEn = "Free diagnostics of your device issue.",
                DescriptionSq = "Kontrollojmë pajisjen tuaj dhe ju japim vlerësim të saktë të kostos para riparimit.",
                DescriptionEn = "We check your device and give you an accurate cost estimate before repair.",
                PriceNoteSq = "Falas", PriceNoteEn = "Free",
                IsPublished = true
            }
        );
    }
}
