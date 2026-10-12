using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;
using EmergencyCart.Domain.AccountContext.Enums;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Complete;

public sealed class Handler(
    IInspectionRepository inspectionRepository,
    IUnitOfWork ofWork) : ICommandHandler<Command, Response>
{
    public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
    {
        var validationResult = Validate(request); 
        if (validationResult.IsFailure)
            return Result.Failure<Response>(validationResult.Error);

        var inspection = await inspectionRepository.GetByIdWithItemsAsync(request.InspectionId, cancellationToken);
        if (inspection is null)
            return Result.Failure<Response>(Error.NotFound("404", "Inspection not found"));

        if (inspection.EmployeeId != request.EmployeeId)
            return Result.Failure<Response>(Error.Forbidden("403", "Only the employee who initiated the inspection can complete it"));

        if (inspection.Status != InspectionStatus.InProgress)
            return Result.Failure<Response>(Error.Conflict("409", "The inspection has already been completed"));

        var pendingItems = inspection.InspectionItems.Count(x => x.FoundQuantity is null);
        if (pendingItems > 0)
            return Result.Failure<Response>(Error.BadRequest("400", $"There are still {pendingItems} items without a registered quantity"));

        inspection.Complete();
        await ofWork.CommitAsync(cancellationToken);

        return Result.Success(new Response("Inspection completed successfully"));
    }

    private static Result Validate(Command request)
    {
        var validation = new Validator();
        var result = validation.Validate(request);
        if (result.IsValid)
            return Result.Success();

        var errors = result.Errors.Select(e => e.ErrorMessage).ToArray();
        return Result.Failure<Response>(new ValidationError(errors));
    }
}
