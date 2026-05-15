namespace ServisBinjaket.Application.Common;

public static class LocalizationHelper
{
    public static string Resolve(string sq, string? en, string locale) =>
        locale == "en" && !string.IsNullOrWhiteSpace(en) ? en : sq;

    public static string? ResolveNullable(string? sq, string? en, string locale) =>
        locale == "en" && !string.IsNullOrWhiteSpace(en) ? en : sq;

    public static string NormalizeLocale(string? locale) =>
        locale == "en" ? "en" : "sq";
}
