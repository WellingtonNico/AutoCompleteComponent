using Microsoft.AspNetCore.Components;

namespace AutoComplete;

partial class StaticSingleAutoComplete<TId, TData>
    : StaticAutoCompleteBase<AutoCompleteOption<TId, TData>, TId, TData>
{
    [Parameter]
    public RenderFragment<AutoCompleteOption<TId, TData>>? SelectedItemTemplate { get; set; }

    public override List<TId> GetSelectedIds()
    {
        if (Value == null)
        {
            return [];
        }
        else
        {
            return [Value.Id];
        }
    }

    public override async Task OnSelectOptionAsync(AutoCompleteOption<TId, TData> option)
    {
        if (Disabled)
            return;
        Value = option;
        await ValueChanged.InvokeAsync(Value);
        if (CloseDropdownOnSelect)
        {
            HideDropdown();
        }
        else
        {
            await SearchInputRef.FocusAsync();
        }
    }
}
