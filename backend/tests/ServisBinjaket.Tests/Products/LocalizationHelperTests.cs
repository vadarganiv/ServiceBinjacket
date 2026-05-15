using FluentAssertions;
using ServisBinjaket.Application.Common;

namespace ServisBinjaket.Tests.Products;

public class LocalizationHelperTests
{
    [Fact]
    public void Resolve_LocaleSq_ReturnsSq()
    {
        LocalizationHelper.Resolve("Telefonë", "Phones", "sq").Should().Be("Telefonë");
    }

    [Fact]
    public void Resolve_LocaleEn_WithEnValue_ReturnsEn()
    {
        LocalizationHelper.Resolve("Telefonë", "Phones", "en").Should().Be("Phones");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_LocaleEn_EnEmpty_FallsBackToSq(string? en)
    {
        LocalizationHelper.Resolve("Telefonë", en, "en").Should().Be("Telefonë");
    }

    [Fact]
    public void Resolve_UnknownLocale_ReturnsSq()
    {
        LocalizationHelper.Resolve("Telefonë", "Phones", "fr").Should().Be("Telefonë");
    }

    [Fact]
    public void NormalizeLocale_En_ReturnsEn()
    {
        LocalizationHelper.NormalizeLocale("en").Should().Be("en");
    }

    [Theory]
    [InlineData("sq")]
    [InlineData(null)]
    [InlineData("fr")]
    [InlineData("")]
    public void NormalizeLocale_NonEn_ReturnsSq(string? locale)
    {
        LocalizationHelper.NormalizeLocale(locale).Should().Be("sq");
    }
}
