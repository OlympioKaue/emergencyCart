using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;
using EmergencyCart.Domain.AccountContext.Enums;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.GetPending;

public sealed class Handler(IInspectionRepository inspectionRepository) : IQueryHandler<Query, IReadOnlyList<Response>>
{
    public async Task<Result<IReadOnlyList<Response>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var inspections = await inspectionRepository.GetLastCompletedOfEachCartWithItemDetailsAsync(cancellationToken);

        var response = inspections
        .Select(inspection => new Response(
          inspection.EmergencyCartId,
          inspection.EmergencyCart.Code,
          inspection.EmergencyCart.Sector.Names,
          inspection.Id,
          DateTime.SpecifyKind(inspection.FinishedAt!.Value, DateTimeKind.Utc),
        [.. inspection.InspectionItems
            .Where(x => x.PendingQuantity > 0)
            .Select(x => new PendingItemResponse(
                x.ItemId,
                x.Item.Names,
                x.ExpectedQuantity,
                x.FoundQuantity!.Value,
                x.MissingQuantity,
                x.ReplenishedQuantity,
                x.PendingQuantity,
                x.Note))
        .OrderBy(x => x.ItemName)]))
        .Where(r => r.Items.Count > 0)
        .OrderBy(r => r.SectorName)
        .ToList();

        return Result.Success<IReadOnlyList<Response>>(response);
    }
}