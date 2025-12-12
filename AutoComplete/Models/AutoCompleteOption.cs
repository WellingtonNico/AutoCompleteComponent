namespace AutoComplete;

public record AutoCompleteOption<TId, TData>
{
    public TId Id { get; set; }
    public TData Data { get; set; }
    public string? SelectedLabel { get; set; }
    public string? DropdownLabel { get; set; }

    public string GetSelectedLabel()
    {
        return SelectedLabel ?? DropdownLabel ?? Id?.ToString() ?? string.Empty;
    }

    public string GetDropdownLabel()
    {
        return DropdownLabel ?? SelectedLabel ?? Id?.ToString() ?? string.Empty;
    }
}
