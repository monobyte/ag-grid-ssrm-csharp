using BondTradingApi.Models;

namespace BondTradingApi.Services;

public interface IGenericGridService<T>
{
    Task<ServerSideResponse> GetRowsAsync(ServerSideRequest request);
}

public abstract class GenericGridService<T> : IGenericGridService<T>
{
    protected abstract List<T> GetAllData();
    protected abstract object ConvertToGridRow(T item);
    protected abstract bool IsGroupItem(T item);
    protected abstract List<T> GetChildItems(string parentKey);
    
    public virtual async Task<ServerSideResponse> GetRowsAsync(ServerSideRequest request)
    {
        await Task.Delay(1); // Simulate async operation

        var allData = GetAllData();
        var filteredData = GenericDataService.ApplyFilters(allData, request.FilterModel);
        var sortedData = GenericDataService.ApplySorting(filteredData, request.SortModel);

        Console.WriteLine($"Generic filter applied: {allData.Count} -> {filteredData.Count} items");
        Console.WriteLine($"Generic sorting applied: {request.SortModel.Count} sort criteria");

        // Handle expansion (if group keys exist but no grouping columns)
        if (request.GroupingCols.Count == 0 && request.GroupKeys.Count > 0)
        {
            var parentKey = request.GroupKeys.Last();
            var childItems = GetChildItems(parentKey);
            var sortedChildren = GenericDataService.ApplySorting(childItems, request.SortModel);
            
            var pagedChildren = sortedChildren
                .Skip(request.StartRow)
                .Take(request.EndRow - request.StartRow)
                .Select(ConvertToGridRow)
                .ToList();

            return new ServerSideResponse
            {
                Rows = pagedChildren,
                LastRow = sortedChildren.Count
            };
        }

        // Handle regular data display
        var parentItems = sortedData.Where(IsGroupItem).ToList();
        var pagedItems = parentItems
            .Skip(request.StartRow)
            .Take(request.EndRow - request.StartRow)
            .Select(ConvertToGridRow)
            .ToList();

        return new ServerSideResponse
        {
            Rows = pagedItems,
            LastRow = parentItems.Count
        };
    }
}