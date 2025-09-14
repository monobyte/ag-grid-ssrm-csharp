using BondTradingApi.Models;

namespace BondTradingApi.Services;

public class TickSimulationService : BackgroundService
{
    private readonly IBondService _bondService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TickSimulationService> _logger;
    private readonly Random _random = new();

    public TickSimulationService(
        IBondService bondService, 
        ISubscriptionService subscriptionService,
        IConfiguration configuration,
        ILogger<TickSimulationService> logger)
    {
        _bondService = bondService;
        _subscriptionService = subscriptionService;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tickInterval = _configuration.GetValue<int>("BondSettings:TickIntervalMs", 250);
        var updatePercentage = _configuration.GetValue<double>("BondSettings:UpdatePercentage", 0.02);

        _logger.LogInformation($"Starting tick simulation with {tickInterval}ms interval and {updatePercentage:P} update rate");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SimulateTick(updatePercentage);
                await Task.Delay(tickInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during tick simulation");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    private async Task SimulateTick(double updatePercentage)
    {
        var allBonds = await _bondService.GetAllBondsAsync();
        var bondsToUpdate = allBonds
            .Where(_ => _random.NextDouble() < updatePercentage)
            .ToList();

        foreach (var bond in bondsToUpdate)
        {
            UpdateBondPrices(bond);
            _bondService.UpdateBond(bond);
            await _subscriptionService.BufferBondUpdateAsync(bond);
        }

        if (bondsToUpdate.Any())
        {
            _logger.LogDebug($"Updated {bondsToUpdate.Count} bonds");
        }
    }

    private void UpdateBondPrices(Bond bond)
    {
        var bidChangePercent = (_random.NextDouble() - 0.5) * 0.02;
        var askChangePercent = (_random.NextDouble() - 0.5) * 0.02;
        var yieldChangePercent = (_random.NextDouble() - 0.5) * 0.01;

        bond.Bid = Math.Max(0.01m, bond.Bid * (1 + (decimal)bidChangePercent));
        bond.Ask = Math.Max(bond.Bid + 0.01m, bond.Ask * (1 + (decimal)askChangePercent));
        bond.Yield = Math.Max(0.01m, bond.Yield * (1 + (decimal)yieldChangePercent));
        bond.LastPrice = (bond.Bid + bond.Ask) / 2;
        bond.Volume += _random.Next(-50, 100);
        bond.Volume = Math.Max(0, bond.Volume);
        bond.UpdateTime = DateTime.UtcNow;
    }
}