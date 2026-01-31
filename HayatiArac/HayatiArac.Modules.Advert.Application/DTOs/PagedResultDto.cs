namespace HayatiArac.Modules.Advert.Application.DTOs;

public sealed record PagedResultDto<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
