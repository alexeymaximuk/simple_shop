using Shop.Shared.Constants;
using Shop.Shared.Interfaces;
using Shop.Shared.Settings;
using StackExchange.Redis;

namespace Shop.Shared.Services;

public class RedisSessionService(IConnectionMultiplexer redis, RedisSettings settings) : IRedisSessionService
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly TimeSpan _sessionDuration = TimeSpan.FromMinutes(settings.SessionDurationMinutes);
    
    public async Task SetSessionAsync(Guid guid, string userId)
    => await _db.StringSetAsync(GetRedisKey(guid), userId, _sessionDuration);

    public async Task<string?> GetSessionAsync(Guid guid)
    => await _db.StringGetAsync(GetRedisKey(guid));

    public async Task RefreshSessionAsync(Guid guid)
        => await _db.KeyExpireAsync(GetRedisKey(guid), _sessionDuration);

    public async Task RemoveSessionAsync(Guid guid)
    => await _db.KeyDeleteAsync(GetRedisKey(guid));

    private static string GetRedisKey(Guid guid) => $"{AuthConstants.SessionKeyPrefix}{guid}";
}