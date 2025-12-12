using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AutoComplete;

public static class AutoCompleteExtensions
{
    public static List<AutoCompleteOption<TSource, TSource>> ToAutoCompleteOptions<TSource>()
        where TSource : Enum
    {
        var values = Enum.GetValues(typeof(TSource)).Cast<TSource>();
        return values
            .Select(v => new AutoCompleteOption<TSource, TSource>
            {
                Id = (TSource)Convert.ChangeType(v, typeof(TSource)),
                Data = v,
                DropdownLabel = GetDisplayName(v),
                SelectedLabel = GetDisplayName(v),
            })
            .ToList();
    }

    public static List<AutoCompleteOption<TSource, TSource>> ToAutoCompleteOptions<TSource>(
        this TSource _
    )
        where TSource : Enum
    {
        return ToAutoCompleteOptions<TSource>();
    }

    private static string GetDisplayName<TSource>(TSource enumValue)
        where TSource : Enum
    {
        var member = enumValue.GetType().GetMember(enumValue.ToString())[0];
        var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.Name ?? enumValue.ToString();
    }

    public static List<AutoCompleteOption<TData, TData>> ToAutoCompleteOptions<TData>(
        this ICollection<TData> objects
    )
    {
        return objects
            .Select(s => new AutoCompleteOption<TData, TData>
            {
                Id = s,
                Data = s,
                DropdownLabel = s?.ToString() ?? string.Empty,
                SelectedLabel = s?.ToString() ?? string.Empty,
            })
            .ToList();
    }
}
