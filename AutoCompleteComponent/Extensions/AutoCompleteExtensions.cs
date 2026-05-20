using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AutoCompleteComponent;

public static class AutoCompleteExtensions
{
    private static string GetDisplayName<TSource>(TSource enumValue)
        where TSource : Enum
    {
        var member = enumValue.GetType().GetMember(enumValue.ToString())[0];
        var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.Name ?? enumValue.ToString();
    }

    public static List<AutoCompleteOption<TSource, TSource>> ToAutoCompleteOptions<TSource>()
        where TSource : Enum
    {
        var values = Enum.GetValues(typeof(TSource)).Cast<TSource>();
        return
        [
            .. values.Select(source =>
            {
                var displayName = GetDisplayName(source);
                return new AutoCompleteOption<TSource, TSource>
                {
                    Id = (TSource)Convert.ChangeType(source, typeof(TSource)),
                    Data = source,
                    DropdownLabel = displayName,
                    SelectedLabel = displayName,
                };
            }),
        ];
    }

    public static List<AutoCompleteOption<TSource, TSource>> ToAutoCompleteOptions<TSource>(
        this TSource _
    )
        where TSource : Enum
    {
        return ToAutoCompleteOptions<TSource>();
    }

    public static List<AutoCompleteOption<TData, TData>> ToAutoCompleteOptions<TData>(
        this ICollection<TData> objects
    )
    {
        return
        [
            .. objects.Select(source => new AutoCompleteOption<TData, TData>
            {
                Id = source,
                Data = source,
                DropdownLabel = source?.ToString() ?? string.Empty,
                SelectedLabel = source?.ToString() ?? string.Empty,
            }),
        ];
    }
}
