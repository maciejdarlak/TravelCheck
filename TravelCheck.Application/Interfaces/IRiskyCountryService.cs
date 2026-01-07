namespace TravelCheck.Application.Interfaces;

public interface IRiskyCountryService
{
    Task<bool> IsCountryRiskyAsync(string country, CancellationToken ct = default );
}