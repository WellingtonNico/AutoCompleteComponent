namespace AutoCompleteComponent;

public record AutoCompleteSearchArgs(
    string SearchTerm,
    int Skip,
    int Take,
    CancellationToken CancellationToken
);

public static class AutoCompleteSearchArgsExtensions
{
    /// <summary>
    /// Checks if there is someting to search
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static bool IsEmpty(this AutoCompleteSearchArgs args)
    {
        return string.IsNullOrWhiteSpace(args.SearchTerm) && args.Skip == 0;
    }
}
