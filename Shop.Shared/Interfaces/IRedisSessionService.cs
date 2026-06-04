namespace Shop.Shared.Interfaces;

public interface IRedisSessionService
{
    Task SetSessionAsync(Guid guid, string userId);
    Task<string?> GetSessionAsync(Guid guid);
    Task RefreshSessionAsync(Guid guid);
    Task RemoveSessionAsync(Guid guid);
}

