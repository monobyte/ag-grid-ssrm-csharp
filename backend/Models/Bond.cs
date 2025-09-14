namespace BondTradingApi.Models;

public class Bond
{
    public string InstrumentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public DateTime MaturityDate { get; set; }
    public decimal CouponRate { get; set; }
    public decimal FaceValue { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public decimal Spread => Ask - Bid;
    public decimal Yield { get; set; }
    public decimal OpeningPrice { get; set; }
    public decimal ClosingPrice { get; set; }
    public decimal LastPrice { get; set; }
    public int Volume { get; set; }
    public DateTime UpdateTime { get; set; }
    public string Rating { get; set; } = string.Empty;
    public string Isin { get; set; } = string.Empty;
    public string Cusip { get; set; } = string.Empty;
    public string TierId { get; set; } = "Tier1";
}

public class BondRow
{
    public string? InstrumentId { get; set; }
    public string? Name { get; set; }
    public string? Issuer { get; set; }
    public string? Currency { get; set; }
    public string? Sector { get; set; }
    public DateTime? MaturityDate { get; set; }
    public decimal? CouponRate { get; set; }
    public decimal? FaceValue { get; set; }
    public decimal? Bid { get; set; }
    public decimal? Ask { get; set; }
    public decimal? Spread { get; set; }
    public decimal? Yield { get; set; }
    public decimal? OpeningPrice { get; set; }
    public decimal? ClosingPrice { get; set; }
    public decimal? LastPrice { get; set; }
    public int? Volume { get; set; }
    public DateTime? UpdateTime { get; set; }
    public string? Rating { get; set; }
    public string? Isin { get; set; }
    public string? Cusip { get; set; }
    public string? TierId { get; set; }
    public bool IsGroup { get; set; }
    public string? Key { get; set; }
    public int? ChildCount { get; set; }
}

public class GroupRow
{
    public string Key { get; set; } = string.Empty;
    public bool IsGroup { get; set; } = true;
    public int ChildCount { get; set; }
}

public class ServerSideRequest
{
    public int StartRow { get; set; }
    public int EndRow { get; set; }
    public List<SortModel> SortModel { get; set; } = new();
    public object? FilterModel { get; set; } = new();
    public List<string> GroupKeys { get; set; } = new();
    public List<string> GroupingCols { get; set; } = new();
}

public class SortModel
{
    public string ColId { get; set; } = string.Empty;
    public string Sort { get; set; } = string.Empty;
}

public class FilterModel
{
    public string FilterType { get; set; } = string.Empty;
    public object? Filter { get; set; }
    public List<string>? Values { get; set; }
}

public class ServerSideResponse
{
    public List<object> Rows { get; set; } = new();
    public int? LastRow { get; set; }
}

public class SubscriptionFilter
{
    public List<string> Currencies { get; set; } = new();
    public List<string> Sectors { get; set; } = new();
}