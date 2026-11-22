using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetHistory;

public sealed record class Handler(IInspectionRepository InspectionRepository) : IQueryHandler<Query, Response>
{
    public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
    {
        var validationResult = Validate(request);
        if (validationResult.IsFailure)
            return Result.Failure<Response>(validationResult.Error);

        var (inspections, totalCount) = await InspectionRepository.GetCompletedPageAsync(request.EmergencyCartId, request.Page, request.PageSize, cancellationToken);

        var items = inspections
            .Select(i => new HistoryItemResponse(
                i.Id,
                i.EmergencyCartId,
                i.EmergencyCart.Code,
                i.EmergencyCart.Sector.Names,
                $"{i.Employee.Name.FirstName} {i.Employee.Name.LastName}",
                DateTime.SpecifyKind(i.FinishedAt!.Value, DateTimeKind.Utc),
                i.InspectionItems.Count,
                i.InspectionItems.Count(x => x.Comparison == Domain.AccountContext.Enums.ComparisonStatus.Missing)
            )).ToArray();

        return Result.Success(new Response(request.Page, request.PageSize, totalCount, items));
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
