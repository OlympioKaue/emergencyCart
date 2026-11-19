using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetHistory;

public sealed class Handler(IInspectionRepository inspectionRepository)
    : IQueryHandler<Query, IReadOnlyList<Response>>
{
    public async Task<Result<IReadOnlyList<Response>>> Handle(
        Query request,
        CancellationToken cancellationToken)
    {
        var inspections = await inspectionRepository.GetHistoryAsync(cancellationToken);

        var response = inspections.Select(inspection => new Response(
            inspection.Id,
            inspection.EmergencyCartId,
            inspection.EmergencyCart.Code,
            inspection.EmergencyCart.Sector.Names,
            $"{inspection.Employee.Name.FirstName} {inspection.Employee.Name.LastName}".Trim(),
            inspection.Status.ToString(),
            DateTime.SpecifyKind(inspection.StartedAt, DateTimeKind.Utc),
            inspection.FinishedAt.HasValue
                ? DateTime.SpecifyKind(inspection.FinishedAt.Value, DateTimeKind.Utc)
                : null,
            inspection.InspectionItems
                .OrderBy(item => item.Item.Names)
                .Select(item => new ItemResponse(
                    item.Item.Names,
                    item.ExpectedQuantity,
                    item.FoundQuantity,
                    item.MissingQuantity,
                    item.ReplenishedQuantity,
                    item.PendingQuantity,
                    item.Note,
                    item.Replenishments
                        .OrderBy(replenishment => replenishment.ReplenishedAt)
                        .Select(replenishment => new ReplenishmentResponse(
                            replenishment.Quantity,
                            DateTime.SpecifyKind(replenishment.ReplenishedAt, DateTimeKind.Utc),
                            $"{replenishment.Employee.Name.FirstName} {replenishment.Employee.Name.LastName}".Trim()))
                        .ToList()))
                .ToList()))
            .ToList();

        return Result.Success<IReadOnlyList<Response>>(response);
    }
}
