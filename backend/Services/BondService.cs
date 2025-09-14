using System.Collections.Concurrent;
using System.Text.Json;
using BondTradingApi.Models;

namespace BondTradingApi.Services;

public class BondService : IBondService
{
    private readonly ConcurrentDictionary<string, List<Bond>> _bonds = new();
    private readonly Random _random = new();
    private readonly List<string> _currencies = new() { "EUR", "GBP", "USD" };
    private readonly List<string> _sectors = new() { "Government", "Corporate", "Municipal" };
    private readonly List<string> _issuers = new() 
    { 
        "US Government", "German Government", "UK Government", "Apple Inc", "Microsoft Corp",
        "Google Inc", "Goldman Sachs", "JPMorgan Chase", "Bank of America", "Wells Fargo",
        "City of New York", "State of California", "Municipality of London"
    };
    private readonly List<string> _ratings = new() { "AAA", "AA+", "AA", "AA-", "A+", "A", "A-", "BBB+", "BBB", "BBB-" };

    public BondService(IConfiguration configuration)
    {
        var bondCount = configuration.GetValue<int>("BondSettings:BondCount", 3000);
        GenerateMockBonds(bondCount);
    }

    private void GenerateMockBonds(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var instrumentId = $"BOND{i:D4}";
            var bonds = new List<Bond>();

            var baseBond = CreateBaseBond(instrumentId, i);
            bonds.Add(baseBond);

            for (int tier = 2; tier <= 5; tier++)
            {
                var tierBond = CreateTierBond(baseBond, $"Tier{tier}");
                bonds.Add(tierBond);
            }

            _bonds[instrumentId] = bonds;
        }
    }

    private Bond CreateBaseBond(string instrumentId, int index)
    {
        var currency = GetWeightedCurrency();
        var sector = GetWeightedSector();
        var issuer = _issuers[_random.Next(_issuers.Count)];
        
        var basePrice = _random.Next(80, 120) + (decimal)_random.NextDouble();
        var spread = (decimal)(_random.NextDouble() * 2 + 0.1);

        return new Bond
        {
            InstrumentId = instrumentId,
            Name = $"{issuer} Bond {index + 1}",
            Issuer = issuer,
            Currency = currency,
            Sector = sector,
            MaturityDate = DateTime.Today.AddYears(_random.Next(1, 31)),
            CouponRate = (decimal)(_random.NextDouble() * 8 + 1),
            FaceValue = 1000,
            Bid = basePrice - spread / 2,
            Ask = basePrice + spread / 2,
            Yield = (decimal)(_random.NextDouble() * 6 + 1),
            OpeningPrice = basePrice - (decimal)(_random.NextDouble() * 2 - 1),
            ClosingPrice = basePrice + (decimal)(_random.NextDouble() * 2 - 1),
            LastPrice = basePrice,
            Volume = _random.Next(100, 10000),
            UpdateTime = DateTime.UtcNow,
            Rating = _ratings[_random.Next(_ratings.Count)],
            Isin = $"US{instrumentId}",
            Cusip = $"{instrumentId}7",
            TierId = "Tier1"
        };
    }

    private Bond CreateTierBond(Bond baseBond, string tierId)
    {
        var tierMultiplier = tierId switch
        {
            "Tier2" => 0.95m,
            "Tier3" => 0.9m,
            "Tier4" => 0.85m,
            "Tier5" => 0.8m,
            _ => 1m
        };

        var newSpread = (baseBond.Ask - baseBond.Bid) * tierMultiplier;
        var midPrice = (baseBond.Ask + baseBond.Bid) / 2;

        return new Bond
        {
            InstrumentId = baseBond.InstrumentId,
            Name = baseBond.Name,
            Issuer = baseBond.Issuer,
            Currency = baseBond.Currency,
            Sector = baseBond.Sector,
            MaturityDate = baseBond.MaturityDate,
            CouponRate = baseBond.CouponRate,
            FaceValue = baseBond.FaceValue,
            Bid = midPrice - newSpread / 2,
            Ask = midPrice + newSpread / 2,
            Yield = baseBond.Yield + (decimal)(_random.NextDouble() * 0.4 - 0.2),
            OpeningPrice = baseBond.OpeningPrice,
            ClosingPrice = baseBond.ClosingPrice,
            LastPrice = baseBond.LastPrice,
            Volume = (int)(baseBond.Volume * tierMultiplier),
            UpdateTime = DateTime.UtcNow,
            Rating = baseBond.Rating,
            Isin = baseBond.Isin,
            Cusip = baseBond.Cusip,
            TierId = tierId
        };
    }

    private string GetWeightedCurrency()
    {
        var rand = _random.NextDouble();
        return rand switch
        {
            < 0.4 => "EUR",
            < 0.7 => "GBP", 
            _ => "USD"
        };
    }

    private string GetWeightedSector()
    {
        var rand = _random.NextDouble();
        return rand switch
        {
            < 0.5 => "Government",
            < 0.8 => "Corporate",
            _ => "Municipal"
        };
    }

    public async Task<ServerSideResponse> GetBondRowsAsync(ServerSideRequest request)
    {
        await Task.Delay(1);

        var allBonds = _bonds.Values.SelectMany(b => b).ToList();
        var filteredBonds = GenericDataService.ApplyFilters(allBonds, request.FilterModel);
        var sortedBonds = GenericDataService.ApplySorting(filteredBonds, request.SortModel);

        // Console.WriteLine($"Filter applied: {allBonds.Count} -> {filteredBonds.Count} bonds");
        // Console.WriteLine($"Sorting applied: {request.SortModel.Count} sort criteria");
        // Console.WriteLine($"GroupingCols: [{string.Join(", ", request.GroupingCols)}], GroupKeys: [{string.Join(", ", request.GroupKeys)}]");

        if (request.GroupingCols.Count == 0)
        {
            // If no grouping columns but we have group keys, it means a bond is being expanded
            if (request.GroupKeys.Count > 0)
            {
                var instrumentId = request.GroupKeys.Last();
                var tierRows = sortedBonds
                    .Where(b => b.InstrumentId == instrumentId && b.TierId != "Tier1")
                    .Skip(request.StartRow)
                    .Take(request.EndRow - request.StartRow)
                    .Select(b => new BondRow
                    {
                        InstrumentId = b.InstrumentId,
                        Name = b.Name,
                        Issuer = b.Issuer,
                        Currency = b.Currency,
                        Sector = b.Sector,
                        MaturityDate = b.MaturityDate,
                        CouponRate = b.CouponRate,
                        FaceValue = b.FaceValue,
                        Bid = b.Bid,
                        Ask = b.Ask,
                        Spread = b.Spread,
                        Yield = b.Yield,
                        OpeningPrice = b.OpeningPrice,
                        ClosingPrice = b.ClosingPrice,
                        LastPrice = b.LastPrice,
                        Volume = b.Volume,
                        UpdateTime = b.UpdateTime,
                        Rating = b.Rating,
                        Isin = b.Isin,
                        Cusip = b.Cusip,
                        TierId = b.TierId,
                        IsGroup = false
                    })
                    .Cast<object>()
                    .ToList();

                var totalTiers = sortedBonds
                    .Where(b => b.InstrumentId == instrumentId && b.TierId != "Tier1")
                    .Count();

                return new ServerSideResponse
                {
                    Rows = tierRows,
                    LastRow = totalTiers
                };
            }

            // No grouping, show top-level bonds as expandable groups
            var pagedBonds = sortedBonds
                .Where(b => b.TierId == "Tier1")
                .Skip(request.StartRow)
                .Take(request.EndRow - request.StartRow)
                .Select(b => new BondRow
                {
                    InstrumentId = b.InstrumentId,
                    Name = b.Name,
                    Issuer = b.Issuer,
                    Currency = b.Currency,
                    Sector = b.Sector,
                    MaturityDate = b.MaturityDate,
                    CouponRate = b.CouponRate,
                    FaceValue = b.FaceValue,
                    Bid = b.Bid,
                    Ask = b.Ask,
                    Spread = b.Spread,
                    Yield = b.Yield,
                    OpeningPrice = b.OpeningPrice,
                    ClosingPrice = b.ClosingPrice,
                    LastPrice = b.LastPrice,
                    Volume = b.Volume,
                    UpdateTime = b.UpdateTime,
                    Rating = b.Rating,
                    Isin = b.Isin,
                    Cusip = b.Cusip,
                    TierId = b.TierId,
                    IsGroup = true
                })
                .Cast<object>()
                .ToList();

            return new ServerSideResponse
            {
                Rows = pagedBonds,
                LastRow = sortedBonds.Count(b => b.TierId == "Tier1")
            };
        }

        return HandleGroupedRequest(sortedBonds, request);
    }

    private ServerSideResponse HandleGroupedRequest(List<Bond> bonds, ServerSideRequest request)
    {
        var groupingLevel = request.GroupKeys.Count;
        var groupingCols = request.GroupingCols;

        if (groupingLevel < groupingCols.Count)
        {
            var currentGroupCol = groupingCols[groupingLevel];
            var groups = bonds
                .Where(b => b.TierId == "Tier1")
                .Where(b => MatchesGroupKeys(b, request.GroupKeys, groupingCols))
                .GroupBy(b => GetGroupValue(b, currentGroupCol))
                .Skip(request.StartRow)
                .Take(request.EndRow - request.StartRow)
                .Select(g => new GroupRow
                {
                    Key = g.Key,
                    IsGroup = true,
                    ChildCount = g.Count()
                })
                .Cast<object>()
                .ToList();

            var totalGroups = bonds
                .Where(b => b.TierId == "Tier1")
                .Where(b => MatchesGroupKeys(b, request.GroupKeys, groupingCols))
                .GroupBy(b => GetGroupValue(b, currentGroupCol))
                .Count();

            return new ServerSideResponse
            {
                Rows = groups,
                LastRow = totalGroups
            };
        }
        else if (groupingLevel == groupingCols.Count)
        {
            var bondRows = bonds
                .Where(b => b.TierId == "Tier1")
                .Where(b => MatchesGroupKeys(b, request.GroupKeys, groupingCols))
                .Skip(request.StartRow)
                .Take(request.EndRow - request.StartRow)
                .Select(b => new BondRow
                {
                    InstrumentId = b.InstrumentId,
                    Name = b.Name,
                    Issuer = b.Issuer,
                    Currency = b.Currency,
                    Sector = b.Sector,
                    MaturityDate = b.MaturityDate,
                    CouponRate = b.CouponRate,
                    FaceValue = b.FaceValue,
                    Bid = b.Bid,
                    Ask = b.Ask,
                    Spread = b.Spread,
                    Yield = b.Yield,
                    OpeningPrice = b.OpeningPrice,
                    ClosingPrice = b.ClosingPrice,
                    LastPrice = b.LastPrice,
                    Volume = b.Volume,
                    UpdateTime = b.UpdateTime,
                    Rating = b.Rating,
                    Isin = b.Isin,
                    Cusip = b.Cusip,
                    TierId = b.TierId,
                    IsGroup = true
                })
                .Cast<object>()
                .ToList();

            var totalBonds = bonds
                .Where(b => b.TierId == "Tier1")
                .Where(b => MatchesGroupKeys(b, request.GroupKeys, groupingCols))
                .Count();

            return new ServerSideResponse
            {
                Rows = bondRows,
                LastRow = totalBonds
            };
        }
        else
        {
            var instrumentId = request.GroupKeys.Last();
            var tierRows = bonds
                .Where(b => b.InstrumentId == instrumentId && b.TierId != "Tier1")
                .Skip(request.StartRow)
                .Take(request.EndRow - request.StartRow)
                .Select(b => new BondRow
                {
                    InstrumentId = b.InstrumentId,
                    Name = b.Name,
                    Issuer = b.Issuer,
                    Currency = b.Currency,
                    Sector = b.Sector,
                    MaturityDate = b.MaturityDate,
                    CouponRate = b.CouponRate,
                    FaceValue = b.FaceValue,
                    Bid = b.Bid,
                    Ask = b.Ask,
                    Spread = b.Spread,
                    Yield = b.Yield,
                    OpeningPrice = b.OpeningPrice,
                    ClosingPrice = b.ClosingPrice,
                    LastPrice = b.LastPrice,
                    Volume = b.Volume,
                    UpdateTime = b.UpdateTime,
                    Rating = b.Rating,
                    Isin = b.Isin,
                    Cusip = b.Cusip,
                    TierId = b.TierId,
                    IsGroup = false
                })
                .Cast<object>()
                .ToList();

            var totalTiers = bonds
                .Where(b => b.InstrumentId == instrumentId && b.TierId != "Tier1")
                .Count();

            return new ServerSideResponse
            {
                Rows = tierRows,
                LastRow = totalTiers
            };
        }
    }

    private bool MatchesGroupKeys(Bond bond, List<string> groupKeys, List<string> groupingCols)
    {
        for (int i = 0; i < groupKeys.Count; i++)
        {
            if (i >= groupingCols.Count) return false;
            var expectedValue = groupKeys[i];
            var actualValue = GetGroupValue(bond, groupingCols[i]);
            if (actualValue != expectedValue) return false;
        }
        return true;
    }

    private string GetGroupValue(Bond bond, string groupCol)
    {
        return groupCol.ToLower() switch
        {
            "sector" => bond.Sector,
            "currency" => bond.Currency,
            "issuer" => bond.Issuer,
            "rating" => bond.Rating,
            _ => bond.InstrumentId
        };
    }


    public async Task<List<Bond>> GetTiersForBondAsync(string instrumentId)
    {
        await Task.Delay(1);
        
        if (_bonds.TryGetValue(instrumentId, out var bonds))
        {
            return bonds.Where(b => b.TierId != "Tier1").ToList();
        }
        
        return new List<Bond>();
    }

    public async Task<List<Bond>> GetAllBondsAsync()
    {
        await Task.Delay(1);
        return _bonds.Values.SelectMany(b => b).ToList();
    }

    public void UpdateBond(Bond bond)
    {
        if (_bonds.TryGetValue(bond.InstrumentId, out var bonds))
        {
            var existing = bonds.FirstOrDefault(b => b.TierId == bond.TierId);
            if (existing != null)
            {
                existing.Bid = bond.Bid;
                existing.Ask = bond.Ask;
                existing.Yield = bond.Yield;
                existing.LastPrice = bond.LastPrice;
                existing.Volume = bond.Volume;
                existing.UpdateTime = bond.UpdateTime;
            }
        }
    }

    public async Task<List<string>> GetDistinctValuesAsync(string columnName)
    {
        await Task.Delay(1);
        
        var allBonds = _bonds.Values.SelectMany(b => b).Where(b => b.TierId == "Tier1").ToList();
        return GenericDataService.GetDistinctValues(allBonds, columnName);
    }

    public Bond? GetBond(string instrumentId, string tierId = "Tier1")
    {
        if (_bonds.TryGetValue(instrumentId, out var bonds))
        {
            return bonds.FirstOrDefault(b => b.TierId == tierId);
        }
        return null;
    }
}