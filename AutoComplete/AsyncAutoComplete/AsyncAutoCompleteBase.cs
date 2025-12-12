using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AutoComplete;

public abstract class AsyncAutoCompleteBase<TValue, TId, TData>
    : InputBase<TValue>,
        IAsyncDisposable
{
    // Início dos parâmetros compartilhados
    [Parameter]
    public string Placeholder { get; set; } = "Buscar ...";

    [Parameter]
    public string SearchInputPlaceholder { get; set; } = "Digite algo para pesquisar";

    [Parameter]
    public string NoResultsMessage { get; set; } = "Nenhum resultado encontrado";

    [Parameter]
    public string TriggerSearchMessage { get; set; } = "Digite algo para iniciar a busca";

    [Parameter]
    public string LoadingMessage { get; set; } = "Buscando...";

    [Parameter]
    public string LoadingMoreMessage { get; set; } = "Carregando mais...";

    [Parameter]
    public bool CloseDropdownOnSelect { get; set; } = true;

    [Parameter]
    public int MinLength { get; set; } = 0;

    [Parameter]
    public bool AutoLoad { get; set; } = false;

    [Parameter]
    public bool Disabled { get; set; } = false;

    [Parameter]
    public Func<
        AutoCompleteSearchArgs,
        Task<List<AutoCompleteOption<TId, TData>>>
    > OnSearch { get; set; }

    [Parameter]
    public RenderFragment<AutoCompleteOption<TId, TData>>? DropdownOptionTemplate { get; set; }

    [Parameter]
    public int PageSize { get; set; } = 10;

    // Fim dos parâmetros compartilhados

    [Inject]
    public IJSRuntime JSRuntime { get; set; }
    public List<AutoCompleteOption<TId, TData>> Options = [];
    public bool WasSearchTriggered = false;
    public bool IsLoading = false;
    public CancellationTokenSource? DebounceTokenSource;
    public ElementReference ContainerRef = new();
    public ElementReference SearchInputRef = new();
    public ElementReference DropdownWrapperRef = new();
    public ElementReference DropdownOptionsListRef = new();
    public bool IsMouseOverDropdown = false;
    public bool IsSearchInputOnFocus = false;
    public bool IsDropdownVisible => IsMouseOverDropdown || IsSearchInputOnFocus;
    public bool ShouldOpenUpward = false;
    public int CurrentPage = 1;
    public bool HasMoreItems = true;
    public bool IsLoadingMore = false;
    public string CurrentSearchText = string.Empty;
    public bool ValidationTriggered = false;

    public List<AutoCompleteOption<TId, TData>> RenderedOptions
    {
        get
        {
            var selectedIds = GetSelectedIds();
            return Options.Where(o => !selectedIds.Contains(o.Id)).ToList();
        }
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

    public virtual async Task<List<AutoCompleteOption<TId, TData>>> SearchItems(
        string searchText,
        CancellationToken cancellationToken
    )
    {
        try
        {
            WasSearchTriggered = true;
            IsLoading = true;
            CurrentSearchText = searchText.Replace("  ", " ").Trim();
            IsLoadingMore = false;
            CurrentPage = 1;
            HasMoreItems = false;
            await InvokeAsync(StateHasChanged);
            var result = await OnSearch(new(CurrentSearchText, 0, PageSize, cancellationToken));
            HasMoreItems = result.Count == PageSize;
            return result;
        }
        finally
        {
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    public virtual async Task LoadMoreItemsAsync()
    {
        if (IsLoadingMore || !HasMoreItems)
            return;

        IsLoadingMore = true;
        CurrentPage++;
        await InvokeAsync(StateHasChanged);

        try
        {
            var result = await OnSearch(
                new(
                    CurrentSearchText,
                    (CurrentPage - 1) * PageSize,
                    PageSize,
                    CancellationToken.None
                )
            );
            HasMoreItems = result.Count == PageSize;

            // Filter out duplicates based on Id
            var existingIds = Options.Select(o => o.Id).ToHashSet();
            Options.AddRange(result.Where(item => !existingIds.Contains(item.Id)));
        }
        catch
        {
            CurrentPage--; // Revert page increment on error
        }
        finally
        {
            IsLoadingMore = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    public virtual async Task ShowDropdownAsync()
    {
        if (Disabled)
            return;
        IsSearchInputOnFocus = true;
        ShouldOpenUpward = await ShouldOpenUpwardAsync();
        await InvokeAsync(StateHasChanged);
        await Task.Delay(10);
        await SearchInputRef.FocusAsync();
        if (AutoLoad && !WasSearchTriggered)
        {
            Options = await SearchItems(string.Empty, CancellationToken.None);
        }
    }

    public virtual void HideDropdown()
    {
        IsSearchInputOnFocus = false;
        IsMouseOverDropdown = false;
    }

    public virtual async Task OnOptionsListScrollAsync()
    {
        if (await IsOptionsListScrolledToBottomAsync())
        {
            await LoadMoreItemsAsync();
        }
    }

    public void OnKeyDownSearchInput(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            HideDropdown();
        }
    }

    public virtual async Task OnSearchInputChangedAsync(ChangeEventArgs e)
    {
        string searchText = e.Value?.ToString() ?? "";

        DebounceTokenSource?.Cancel();
        DebounceTokenSource = new();

        if (searchText.Length >= MinLength)
        {
            try
            {
                await Task.Delay(500, DebounceTokenSource.Token);
                Options = await SearchItems(searchText, DebounceTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected exception, ignore
            }
        }
        else
        {
            Options = [];
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

    public async Task<bool> IsOptionsListScrolledToBottomAsync()
    {
        try
        {
            var result = await JSRuntime.InvokeAsync<bool>(
                "isAutoCompleteOptionsListScrolledToBottom",
                DropdownOptionsListRef
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

    public ValueTask DisposeAsync()
    {
        DebounceTokenSource?.Cancel();
        DebounceTokenSource?.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
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
