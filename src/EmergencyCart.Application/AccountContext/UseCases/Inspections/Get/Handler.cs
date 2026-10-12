using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;
using EmergencyCart.Domain.AccountContext.Enums;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get;

public sealed class Handler(IInspectionRepository inspectionRepository) : IQueryHandler<Query, Response>
{
    public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
    {
        var validationResult = Validate(request);
        if (validationResult.IsFailure)
            return Result.Failure<Response>(validationResult.Error);

        var inspection = await inspectionRepository.GetByIdWithItemDetailsAsync(request.InspectionId, cancellationToken);
        if (inspection is null)
            return Result.Failure<Response>(Error.NotFound("404", "Inspection not found"));

        var items = inspection.InspectionItems
            .Where(x => !request.OnlyMissing || x.Comparison == ComparisonStatus.Missing)
            .Select(x => new ItemResponse(
                x.ItemId,
                x.Item.Names,
                x.ExpectedQuantity,
                x.FoundQuantity,
                x.Difference,
                x.Comparison,
                x.MissingQuantity))
            .ToArray();

        return Result.Success(new Response(inspection.Id, inspection.Status, items));
    }

    private static Result Validate(Query query)
    {
        var validation = new Validator();
        var result = validation.Validate(query);

        if (result.IsValid)
            return Result.Success();

        var errors = result.Errors.Select(e => e.ErrorMessage).ToArray();
        return Result.Failure<Response>(new ValidationError(errors));
    }
}
