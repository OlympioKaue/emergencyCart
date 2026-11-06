using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetLastCompleted;

public sealed class Handler(IInspectionRepository inspectionRepository) : IQueryHandler<Query, Response>
{
    public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
    {
        var validationResult = Validate(request);
        if (validationResult.IsFailure)
            return Result.Failure<Response>(validationResult.Error);

        var inspection = await inspectionRepository.GetLastCompletedByEmergencyCartIdAsync(request.EmergencyCartId, cancellationToken);
        if (inspection is null || inspection.FinishedAt is null)
            return Result.Failure<Response>(Error.NotFound("404", "There is no completed inspection for this cart."));

        var finishedAt = DateTime.SpecifyKind(inspection.FinishedAt.Value, DateTimeKind.Utc);

        return Result.Success(new Response(inspection.Id, finishedAt));
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
