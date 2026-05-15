namespace ServisBinjaket.Application.Services.Queries;

public record GetServicesQuery
{
    public string Locale { get; init; } = "sq";
    public int? CategoryId { get; init; }
}
