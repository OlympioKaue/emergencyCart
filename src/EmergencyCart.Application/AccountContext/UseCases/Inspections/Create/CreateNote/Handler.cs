using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;
using EmergencyCart.Domain.AccountContext.Enums;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Create.CreateNote;

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

        if (inspection.Status != InspectionStatus.Completed)
            return Result.Failure<Response>(Error.Conflict("409", "Only completed inspections can have notes"));

        var lastCompleted = await inspectionRepository.GetLastCompletedByEmergencyCartIdAsync(inspection.EmergencyCartId, cancellationToken);
        if (lastCompleted is null || lastCompleted.Id != inspection.Id)
            return Result.Failure<Response>(Error.Conflict("409", "A more recent inspection exists for this cart"));

        var inspectionItem = inspection.InspectionItems.FirstOrDefault(x => x.ItemId == request.ItemId);
        if (inspectionItem is null)
            return Result.Failure<Response>(Error.NotFound("404", "Item is not part of this inspection"));

        if (inspectionItem.Comparison != ComparisonStatus.Missing)
            return Result.Failure<Response>(Error.Conflict("409", "Only missing items can have a note"));

        inspection.RegisterNote(request.ItemId, request.Note);
        await ofWork.CommitAsync(cancellationToken);

        return Result.Success(new Response("Note saved successfully"));
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