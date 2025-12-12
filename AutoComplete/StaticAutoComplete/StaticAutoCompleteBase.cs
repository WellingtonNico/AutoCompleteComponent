using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AutoComplete;

public abstract class StaticAutoCompleteBase<TValue, TId, TData> : InputBase<TValue>
{
    // Início dos parâmetros compartilhados
    [Parameter]
    public string Placeholder { get; set; } = "Buscar ...";

    [Parameter]
    public string SearchInputPlaceholder { get; set; } = "Digite algo para pesquisar";

    [Parameter]
    public string NoResultsMessage { get; set; } = "Nenhum resultado encontrado";

    [Parameter]
    public bool CloseDropdownOnSelect { get; set; } = true;

    [Parameter]
    public int MinLength { get; set; } = 0;

    [Parameter]
    public bool Disabled { get; set; } = false;

    [Parameter]
    public RenderFragment<AutoCompleteOption<TId, TData>>? DropdownOptionTemplate { get; set; }

    [Parameter]
    public int PageSize { get; set; } = 10;

    [Parameter]
    public List<AutoCompleteOption<TId, TData>> Options { get; set; }

    // Fim dos parâmetros compartilhados

    [Inject]
    public IJSRuntime JSRuntime { get; set; }
    public bool IsDropdownVisible => IsMouseOverDropdown || IsSearchInputOnFocus;
    public ElementReference ContainerRef = new();
    public ElementReference SearchInputRef = new();
    public ElementReference DropdownWrapperRef = new();
    public ElementReference DropdownOptionsListRef = new();
    public bool IsMouseOverDropdown = false;
    public bool IsSearchInputOnFocus = false;
    public bool ShouldOpenUpward = false;
    public bool ValidationTriggered = false;

    public List<AutoCompleteOption<TId, TData>> FilteredOptions = [];

    public List<AutoCompleteOption<TId, TData>> RenderedOptions
    {
        get
        {
            var selectedIds = GetSelectedIds();
            return FilteredOptions.Where(o => !selectedIds.Contains(o.Id)).ToList();
        }
    }

    public virtual async Task OnSearchInputChangedAsync(ChangeEventArgs e)
    {
        string searchText = e.Value?.ToString() ?? "";
        var terms = searchText.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        var filtered = Options.Where(option =>
            // Verifica se o termo corresponde ao rótulo selecionado
            terms.All(term =>
                (option.SelectedLabel ?? "").Contains(term, StringComparison.OrdinalIgnoreCase)
            )
            // Verifica se o termo corresponde ao rótulo do dropdown
            || terms.All(term =>
                option.DropdownLabel!.Contains(term, StringComparison.OrdinalIgnoreCase)
            )
            // Verifica se o termo corresponde ao ID (convertido para string)
            || terms.All(term =>
                string.Equals(option.Id!.ToString(), term, StringComparison.OrdinalIgnoreCase)
            )
        );

        FilteredOptions = filtered.ToList();
        await InvokeAsync(StateHasChanged);
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);
        if (EditContext != null)
        {
            EditContext.OnValidationStateChanged += async (_, __) =>
            {
                ValidationTriggered = true;
                await InvokeAsync(StateHasChanged);
            };
        }
    }

    public virtual async Task ShowDropdown()
    {
        if (Disabled)
            return;
        IsSearchInputOnFocus = true;
        ShouldOpenUpward = await ShouldOpenUpwardAsync();
        await Task.Delay(10);
        await SearchInputRef.FocusAsync();
        FilteredOptions = Options;
        await InvokeAsync(StateHasChanged);
    }

    public virtual void HideDropdown()
    {
        IsSearchInputOnFocus = false;
        IsMouseOverDropdown = false;
    }

    public void OnKeyDownSearchInput(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            HideDropdown();
        }
    }

    public async Task<bool> ShouldOpenUpwardAsync()
    {
        try
        {
            var result = await JSRuntime.InvokeAsync<bool>(
                "shouldOpenAutoCompleteDropdownUpward",
                ContainerRef
            );
            return result;
        }
        catch
        {
            return false;
        }
    }

    public async Task OnClickClearButton()
    {
        if (Disabled)
            return;
        Value = default;
        await ValueChanged.InvokeAsync(Value);
    }

    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out TValue result,
        [NotNullWhen(false)] out string? validationErrorMessage
    )
    {
        validationErrorMessage = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            result = default!;
            return true;
        }

        try
        {
            result = JsonSerializer.Deserialize<TValue>(value)!;
            return true;
        }
        catch (JsonException ex)
        {
            validationErrorMessage = ex.Message;
            result = default;
            return false;
        }
    }

    public void OnMouseMoveOnDropdown()
    {
        IsMouseOverDropdown = true;
    }

    public void OnMouseLeaveDropdown()
    {
        IsMouseOverDropdown = false;
    }

    public void OnBlurSearchInput()
    {
        IsSearchInputOnFocus = false;
    }

    // Abstract methods that must be implemented by derived classes
    public abstract Task OnSelectOptionAsync(AutoCompleteOption<TId, TData> option);
    public abstract List<TId> GetSelectedIds();
}
