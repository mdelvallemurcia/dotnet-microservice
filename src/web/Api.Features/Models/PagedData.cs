namespace Api.Features.Models;

public record PagedData<T>
(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount
);
