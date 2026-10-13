using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetInProgress;

public sealed class Handler(IInspectionRepository inspectionRepository) : IQueryHandler<Query, Response>
{
    public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
    {
        var validationResult = Validate(request);  
        if (validationResult.IsFailure)
            return Result.Failure<Response>(validationResult.Error);

        var inspection = await inspectionRepository.GetInProgressByEmergencyCartIdAsync(request.EmergencyCartId, cancellationToken);
        if (inspection is null)
            return Result.Failure<Response>(Error.NotFound("404", " There is no verification in progress for this cart."));

        if (inspection.EmployeeId != request.EmployeeId)
            return Result.Failure<Response>(Error.Forbidden("403", "Only the employee who started the inspection can resume it."));

        return Result.Success(new Response(inspection.Id));
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
