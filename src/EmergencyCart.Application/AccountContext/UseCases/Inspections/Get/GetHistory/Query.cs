using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetHistory;

public sealed record class Query(Guid? EmergencyCartId, int Page, int PageSize) : IQuery<Response>;