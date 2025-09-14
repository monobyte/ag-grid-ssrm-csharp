using BondTradingApi.Models;

namespace BondTradingApi.Services;

public interface IBondService
{
    Task<ServerSideResponse> GetBondRowsAsync(ServerSideRequest request);
    Task<List<Bond>> GetTiersForBondAsync(string instrumentId);
    Task<List<Bond>> GetAllBondsAsync();
    Task<List<string>> GetDistinctValuesAsync(string columnName);
    void UpdateBond(Bond bond);
    Bond? GetBond(string instrumentId, string tierId = "Tier1");
}