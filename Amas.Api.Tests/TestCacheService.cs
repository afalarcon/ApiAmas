using Amas.Application.Abstractions;

namespace Amas.Api.Tests;

internal sealed class TestCacheService : ICacheService
{
    private readonly Dictionary<string, object> values = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(values.TryGetValue(key, out var value) ? (T?)value : default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        if (value is not null)
        {
            values[key] = value;
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        values.Remove(key);
        return Task.CompletedTask;
    }
}
