namespace Models.Dto;

public record PagedData<T>
(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    long TotalCount
);
