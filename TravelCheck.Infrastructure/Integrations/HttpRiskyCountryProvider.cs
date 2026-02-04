using TravelCheck.Application.Interfaces;

namespace TravelCheck.Infrastructure.Services;

public sealed class HttpRiskyCountryProvider : IRiskyCountryService
{
    private static readonly HashSet<string> RiskyCountries =
    [
        "Afghanistan",
        "Syria",
        "Iran",
        "Iraq"
    ];

    public async Task<bool> IsCountryRiskyAsync(string country, CancellationToken ct = default)
    {
        // Simulate external HTTP call latency
        await Task.Delay(150, ct);

        return RiskyCountries.Contains(country.Trim(), StringComparer.OrdinalIgnoreCase);
    }
}