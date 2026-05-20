using System.ComponentModel.DataAnnotations;

namespace AutoCompleteComponent;

public record AutoCompleteOption<TId, TData>
{
    public required TId Id { get; set; }
    public required TData Data { get; set; }
    public string? SelectedLabel { get; set; }
    public string? DropdownLabel { get; set; }

    private string? GetDataAsString()
    {
        var isDataEnum = typeof(TData).IsEnum;
        if (isDataEnum)
        {
            var displayAttribute =
                typeof(TData)
                    .GetMember(Data?.ToString() ?? string.Empty)[0]
                    .GetCustomAttributes(typeof(DisplayAttribute), false)
                    .FirstOrDefault() as DisplayAttribute;
            return displayAttribute?.Name ?? Data?.ToString();
        }
        else
        {
            return Data?.ToString();
        }
    }

    public string GetSelectedLabel()
    {
        return SelectedLabel ?? DropdownLabel ?? GetDataAsString() ?? string.Empty;
    }

    public string GetDropdownLabel()
    {
        return DropdownLabel ?? SelectedLabel ?? GetDataAsString() ?? string.Empty;
    }
}
