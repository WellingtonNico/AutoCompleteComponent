namespace AutoComplete;

public record AutoCompleteSearchArgs(
    string SearchTerm,
    int Skip,
    int Take,
    CancellationToken CancellationToken
);
