using Microsoft.AspNetCore.SignalR;
using BondTradingApi.Models;
using BondTradingApi.Services;

namespace BondTradingApi.Hubs;

public class BondHub : Hub
{
    private readonly IBondService _bondService;
    private readonly ISubscriptionService _subscriptionService;

    public BondHub(IBondService bondService, ISubscriptionService subscriptionService)
    {
        _bondService = bondService;
        _subscriptionService = subscriptionService;
    }

    public async Task<ServerSideResponse> GetBondRows(ServerSideRequest request)
    {
        return await _bondService.GetBondRowsAsync(request);
    }

    public async Task<List<Bond>> GetTiersForBond(string instrumentId)
    {
        return await _bondService.GetTiersForBondAsync(instrumentId);
    }

    public async Task<List<string>> GetDistinctValues(string columnName)
    {
        return await _bondService.GetDistinctValuesAsync(columnName);
    }

    public async Task SubscribeToFilter(SubscriptionFilter filter)
    {
        await _subscriptionService.SubscribeAsync(Context.ConnectionId, filter);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _subscriptionService.UnsubscribeAsync(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}