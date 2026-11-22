using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetHistory;

public sealed record class Response
    (int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyCollection<HistoryItemResponse> ItemResponses
    ) : IQueryResponse;

