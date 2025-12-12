using Microsoft.AspNetCore.Components;

namespace AutoComplete;

partial class StaticMultiAutoComplete<TId, TData>
    : StaticAutoCompleteBase<List<AutoCompleteOption<TId, TData>>, TId, TData>
{
    public record SelectedItemArgs(AutoCompleteOption<TId, TData> Item, Func<Task> RemoveAction);

    [Parameter]
    public RenderFragment<SelectedItemArgs>? SelectedItemTemplate { get; set; }

    public override List<TId> GetSelectedIds()
    {
        if (Value == null)
        {
            return [];
        }
        else
        {
            return Value.Select(v => v.Id).ToList();
        }
    }

    public override async Task OnSelectOptionAsync(AutoCompleteOption<TId, TData> option)
    {
        if (Disabled)
            return;
        if (Value == null)
        {
            Value = [];
            await ValueChanged.InvokeAsync(Value);
        }

        var isOptionSelected = Value.Any(v => v.Equals(option));
        if (!isOptionSelected)
        {
            Value.Add(option);
            await ValueChanged.InvokeAsync(Value);
        }
        if (CloseDropdownOnSelect)
        {
            HideDropdown();
        }
        else
        {
            await SearchInputRef.FocusAsync();
        }
    }

    public async Task OnClickRemoveOption(AutoCompleteOption<TId, TData> option)
    {
        if (Disabled)
            return;
        Value = Value?.Where(v => !v.Equals(option)).ToList() ?? new();
        await ValueChanged.InvokeAsync(Value);
    }
}
