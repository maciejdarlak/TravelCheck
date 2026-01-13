using TravelCheck.Application.Interfaces;
using TravelCheck.Infrastructure.Resilience;

namespace TravelCheck.Infrastructure.Integrations;

public sealed class RiskyCountryResilienceDecorator : IRiskyCountryService
{
    private readonly IRiskyCountryService _inner;

    public RiskyCountryResilienceDecorator(IRiskyCountryService inner, Microsoft.Extensions.Logging.ILogger<RiskyCountryResilienceDecorator> logger)
    {
        _inner = inner;
    }

    public Task<bool> IsCountryRiskyAsync(string country, CancellationToken ct = default)
    {
        return RiskyCountryPolicies.Combined.ExecuteAsync(
            token => _inner.IsCountryRiskyAsync(country, token),
            ct
        );
    }
}