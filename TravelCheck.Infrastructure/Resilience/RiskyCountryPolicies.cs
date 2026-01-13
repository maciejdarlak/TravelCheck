using Polly;
using Polly.Timeout;

namespace TravelCheck.Infrastructure.Resilience;

public static class RiskyCountryPolicies
{
    // timeout: single call cannot exceed 5 seconds
    public static IAsyncPolicy TimeoutPolicy =>
        Policy.TimeoutAsync(
            TimeSpan.FromSeconds(5),
            TimeoutStrategy.Optimistic);

    // retry :repeats 3 times
    public static IAsyncPolicy RetryPolicy =>
        Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, attempt))
            );

    // circuit breaker: new cycle --> 3 errors --> breaks 30s --> new cycle...
    public static IAsyncPolicy CircuitBreakerPolicy =>
        Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30)
            );

    // Combined policy: Timeout → Retry → Circuit Breaker
    public static IAsyncPolicy Combined =>
        Policy.WrapAsync(
            CircuitBreakerPolicy,
            RetryPolicy,
            TimeoutPolicy
        );
}