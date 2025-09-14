using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using BondTradingApi.Models;
using BondTradingApi.Hubs;

namespace BondTradingApi.Services;

public class SubscriptionService : ISubscriptionService, IDisposable
{
    private readonly ConcurrentDictionary<string, SubscriptionFilter> _subscriptions = new();
    private readonly ConcurrentDictionary<string, BondRow> _bufferedUpdates = new();
    private readonly IHubContext<BondHub> _hubContext;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly Timer _flushTimer;

    public SubscriptionService(IHubContext<BondHub> hubContext, ILogger<SubscriptionService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
        
        // Flush buffered updates every 250ms
        _flushTimer = new Timer(FlushUpdates, null, TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250));
    }

    public async Task SubscribeAsync(string connectionId, SubscriptionFilter filter)
    {
        _subscriptions[connectionId] = filter;
        await Task.CompletedTask;
    }

    public async Task UnsubscribeAsync(string connectionId)
    {
        _subscriptions.TryRemove(connectionId, out _);
        await Task.CompletedTask;
    }

    public async Task BufferBondUpdateAsync(Bond bond)
    {
        var subscribedConnections = GetSubscribedConnections(bond);
        
        if (subscribedConnections.Any())
        {
            var bondRow = new BondRow
            {
                InstrumentId = bond.InstrumentId,
                Name = bond.Name,
                Issuer = bond.Issuer,
                Currency = bond.Currency,
                Sector = bond.Sector,
                MaturityDate = bond.MaturityDate,
                CouponRate = bond.CouponRate,
                FaceValue = bond.FaceValue,
                Bid = bond.Bid,
                Ask = bond.Ask,
                Spread = bond.Spread,
                Yield = bond.Yield,
                OpeningPrice = bond.OpeningPrice,
                ClosingPrice = bond.ClosingPrice,
                LastPrice = bond.LastPrice,
                Volume = bond.Volume,
                UpdateTime = bond.UpdateTime,
                Rating = bond.Rating,
                Isin = bond.Isin,
                Cusip = bond.Cusip,
                TierId = bond.TierId,
                IsGroup = bond.TierId == "Tier1"
            };

            // Buffer the update - key is instrumentId + tierId to handle multiple tiers
            var key = $"{bond.InstrumentId}_{bond.TierId}";
            _bufferedUpdates[key] = bondRow;
        }

        await Task.CompletedTask;
    }

    private async void FlushUpdates(object? state)
    {
        if (_bufferedUpdates.IsEmpty) return;

        // Get all buffered updates and clear the buffer
        var updatesToSend = new List<BondRow>();
        var keys = _bufferedUpdates.Keys.ToList();

        foreach (var key in keys)
        {
            if (_bufferedUpdates.TryRemove(key, out var bondRow))
            {
                updatesToSend.Add(bondRow);
            }
        }

        if (updatesToSend.Any())
        {
            _logger.LogDebug($"Flushing {updatesToSend.Count} bond updates to clients");

            // Group updates by connections that should receive them
            var connectionUpdates = new Dictionary<string, List<BondRow>>();

            foreach (var update in updatesToSend)
            {
                // Find connections that should receive this update
                var connections = _subscriptions
                    .Where(kvp => ShouldReceiveUpdate(update, kvp.Value))
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var connectionId in connections)
                {
                    if (!connectionUpdates.ContainsKey(connectionId))
                    {
                        connectionUpdates[connectionId] = new List<BondRow>();
                    }
                    connectionUpdates[connectionId].Add(update);
                }
            }

            // Send batched updates to each connection
            var tasks = connectionUpdates.Select(async kvp =>
            {
                var connectionId = kvp.Key;
                var updates = kvp.Value;

                try
                {
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("BatchUpdateBonds", updates);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to send updates to connection {connectionId}: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
        }
    }

    private bool ShouldReceiveUpdate(BondRow bondRow, SubscriptionFilter filter)
    {
        if (filter.Currencies.Any() && !filter.Currencies.Contains(bondRow.Currency ?? ""))
            return false;

        if (filter.Sectors.Any() && !filter.Sectors.Contains(bondRow.Sector ?? ""))
            return false;

        return true;
    }

    public List<string> GetSubscribedConnections(Bond bond)
    {
        return _subscriptions
            .Where(kvp => MatchesFilter(bond, kvp.Value))
            .Select(kvp => kvp.Key)
            .ToList();
    }

    private bool MatchesFilter(Bond bond, SubscriptionFilter filter)
    {
        if (filter.Currencies.Any() && !filter.Currencies.Contains(bond.Currency))
            return false;

        if (filter.Sectors.Any() && !filter.Sectors.Contains(bond.Sector))
            return false;

        return true;
    }

    public void Dispose()
    {
        _flushTimer?.Dispose();
    }
}