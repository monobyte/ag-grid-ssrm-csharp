using BondTradingApi.Models;

namespace BondTradingApi.Services;

public interface ISubscriptionService
{
    Task SubscribeAsync(string connectionId, SubscriptionFilter filter);
    Task UnsubscribeAsync(string connectionId);
    Task BufferBondUpdateAsync(Bond bond);
    List<string> GetSubscribedConnections(Bond bond);
}