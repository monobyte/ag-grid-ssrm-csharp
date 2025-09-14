using System.Reflection;
using System.Text.Json;
using BondTradingApi.Models;

namespace BondTradingApi.Services;

public static class GenericDataService
{
    public static List<T> ApplyFilters<T>(List<T> data, object? filterModel)
    {
        if (filterModel == null) return data;

        try
        {
            var filterDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                JsonSerializer.Serialize(filterModel));

            if (filterDict == null) return data;

            foreach (var filter in filterDict)
            {
                var columnId = filter.Key;
                var filterValue = filter.Value;

                if (filterValue is JsonElement jsonElement)
                {
                    data = ApplyColumnFilter(data, columnId, jsonElement);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing filter model: {ex.Message}");
        }

        return data;
    }

    private static List<T> ApplyColumnFilter<T>(List<T> data, string columnId, JsonElement filterElement)
    {
        if (filterElement.ValueKind != JsonValueKind.Object) return data;

        var filterObj = filterElement.Deserialize<Dictionary<string, object>>();
        if (filterObj == null) return data;

        // Get the property info for the column
        var property = GetPropertyInfo<T>(columnId);
        if (property == null) return data;

        // Handle set filters (from our custom filters or AG Grid set filters)
        if (filterObj.ContainsKey("values") && filterObj["values"] is JsonElement valuesElement)
        {
            var values = valuesElement.Deserialize<string[]>();
            if (values != null)
            {
                return ApplySetFilter(data, property, values);
            }
        }

        // Handle text filters from AG Grid
        if (filterObj.ContainsKey("filter") && filterObj["filter"] is JsonElement textFilterElement)
        {
            var filterText = textFilterElement.GetString();
            if (!string.IsNullOrEmpty(filterText))
            {
                var filterType = filterObj.ContainsKey("type") && filterObj["type"] is JsonElement typeElement 
                    ? typeElement.GetString() : "contains";
                
                return ApplyTextFilter(data, property, filterText, filterType ?? "contains");
            }
        }

        // Handle number filters from AG Grid
        if (filterObj.ContainsKey("filter") && filterObj["filter"] is JsonElement numberFilterElement)
        {
            if (numberFilterElement.ValueKind == JsonValueKind.Number)
            {
                var filterValue = numberFilterElement.GetDecimal();
                var filterType = filterObj.ContainsKey("type") && filterObj["type"] is JsonElement typeElement 
                    ? typeElement.GetString() : "equals";
                
                return ApplyNumberFilter(data, property, filterValue, filterType ?? "equals");
            }
        }

        return data;
    }

    private static PropertyInfo? GetPropertyInfo<T>(string columnId)
    {
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        // Console.WriteLine($"Looking for property: '{columnId}' in type: {type.Name}");
        
        // Try exact match first (case insensitive)
        var property = properties.FirstOrDefault(p => 
            string.Equals(p.Name, columnId, StringComparison.OrdinalIgnoreCase));
        
        if (property != null) 
        {
            // Console.WriteLine($"  Found exact match: {property.Name} ({property.PropertyType.Name})");
            return property;
        }

        // Try camelCase to PascalCase conversion (e.g., maturityDate -> MaturityDate)
        var pascalCaseColumnId = ToPascalCase(columnId);
        property = properties.FirstOrDefault(p => 
            string.Equals(p.Name, pascalCaseColumnId, StringComparison.OrdinalIgnoreCase));
        
        if (property != null) 
        {
            // Console.WriteLine($"  Found PascalCase match: {property.Name} ({property.PropertyType.Name})");
            return property;
        }

        // Try with common variations (remove spaces, underscores, etc.)
        var normalizedColumnId = columnId.Replace("_", "").Replace("-", "").Replace(" ", "");
        property = properties.FirstOrDefault(p => 
            string.Equals(p.Name, normalizedColumnId, StringComparison.OrdinalIgnoreCase));

        if (property != null)
        {
            // Console.WriteLine($"  Found normalized match: {property.Name} ({property.PropertyType.Name})");
        }
        else
        {
            // Console.WriteLine($"  No property found for: '{columnId}'");
            // Console.WriteLine($"  Available properties: {string.Join(", ", properties.Select(p => p.Name))}");
        }

        return property;
    }

    private static string ToPascalCase(string camelCase)
    {
        if (string.IsNullOrEmpty(camelCase))
            return camelCase;

        // Convert first character to uppercase
        return char.ToUpper(camelCase[0]) + camelCase.Substring(1);
    }

    private static List<T> ApplySetFilter<T>(List<T> data, PropertyInfo property, string[] values)
    {
        // If no values are selected, exclude all items (empty result)
        if (values.Length == 0)
        {
            return new List<T>();
        }

        return data.Where(item =>
        {
            var propertyValue = property.GetValue(item)?.ToString();
            return propertyValue != null && values.Contains(propertyValue);
        }).ToList();
    }

    private static List<T> ApplyTextFilter<T>(List<T> data, PropertyInfo property, string filterText, string filterType)
    {
        var lowerFilterText = filterText.ToLower();

        Func<string, bool> predicate = filterType switch
        {
            "equals" => value => value.ToLower() == lowerFilterText,
            "notEqual" => value => value.ToLower() != lowerFilterText,
            "contains" => value => value.ToLower().Contains(lowerFilterText),
            "notContains" => value => !value.ToLower().Contains(lowerFilterText),
            "startsWith" => value => value.ToLower().StartsWith(lowerFilterText),
            "endsWith" => value => value.ToLower().EndsWith(lowerFilterText),
            _ => value => value.ToLower().Contains(lowerFilterText)
        };

        return data.Where(item =>
        {
            var propertyValue = property.GetValue(item)?.ToString();
            return propertyValue != null && predicate(propertyValue);
        }).ToList();
    }

    private static List<T> ApplyNumberFilter<T>(List<T> data, PropertyInfo property, decimal filterValue, string filterType)
    {
        return data.Where(item =>
        {
            var propertyValue = property.GetValue(item);
            if (propertyValue == null) return false;

            // Try to convert to decimal for comparison
            decimal numericValue;
            if (propertyValue is decimal decVal)
                numericValue = decVal;
            else if (propertyValue is int intVal)
                numericValue = intVal;
            else if (propertyValue is double doubleVal)
                numericValue = (decimal)doubleVal;
            else if (propertyValue is float floatVal)
                numericValue = (decimal)floatVal;
            else if (decimal.TryParse(propertyValue.ToString(), out var parsedVal))
                numericValue = parsedVal;
            else
                return false;

            return filterType switch
            {
                "equals" => numericValue == filterValue,
                "notEqual" => numericValue != filterValue,
                "lessThan" => numericValue < filterValue,
                "lessThanOrEqual" => numericValue <= filterValue,
                "greaterThan" => numericValue > filterValue,
                "greaterThanOrEqual" => numericValue >= filterValue,
                _ => numericValue == filterValue
            };
        }).ToList();
    }

    public static List<T> ApplySorting<T>(List<T> data, List<SortModel> sortModel)
    {
        if (!sortModel.Any()) return data;

        IOrderedEnumerable<T> orderedData = null!;

        for (int i = 0; i < sortModel.Count; i++)
        {
            var sort = sortModel[i];
            var property = GetPropertyInfo<T>(sort.ColId);
            if (property == null) continue;

            var isAscending = sort.Sort?.ToLower() == "asc";

            if (i == 0)
            {
                // First sort with proper type handling
                orderedData = isAscending 
                    ? data.OrderBy(item => GetComparableValue(property, item))
                    : data.OrderByDescending(item => GetComparableValue(property, item));
            }
            else
            {
                // Subsequent sorts (ThenBy) with proper type handling
                orderedData = isAscending
                    ? orderedData.ThenBy(item => GetComparableValue(property, item))
                    : orderedData.ThenByDescending(item => GetComparableValue(property, item));
            }
        }

        return orderedData.ToList();
    }

    private static object? GetComparableValue<T>(PropertyInfo property, T item)
    {
        var value = property.GetValue(item);
        if (value == null) return null;

        // Handle different data types properly for sorting
        return property.PropertyType switch
        {
            Type t when t == typeof(DateTime) || t == typeof(DateTime?) => value,
            Type t when t == typeof(decimal) || t == typeof(decimal?) => value,
            Type t when t == typeof(int) || t == typeof(int?) => value,
            Type t when t == typeof(double) || t == typeof(double?) => value,
            Type t when t == typeof(float) || t == typeof(float?) => value,
            Type t when t == typeof(long) || t == typeof(long?) => value,
            Type t when t == typeof(short) || t == typeof(short?) => value,
            Type t when t == typeof(byte) || t == typeof(byte?) => value,
            Type t when t == typeof(bool) || t == typeof(bool?) => value,
            // For strings and other types, ensure consistent comparison
            _ => value.ToString() ?? ""
        };
    }

    public static List<string> GetDistinctValues<T>(List<T> data, string columnName)
    {
        var property = GetPropertyInfo<T>(columnName);
        if (property == null) return new List<string>();

        return data
            .Select(item => property.GetValue(item)?.ToString())
            .Where(value => !string.IsNullOrEmpty(value))
            .Distinct()
            .OrderBy(value => value)
            .ToList()!;
    }
}