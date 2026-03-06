namespace Models.Dto;

public record PagedData<T>
(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    long TotalCount
)
{
    public int TotalPages
    {
        get
        {
            if (PageSize < 1) return 0;
            return (int)Math.Ceiling(1.0 * TotalCount / PageSize);
        }
    }
};
