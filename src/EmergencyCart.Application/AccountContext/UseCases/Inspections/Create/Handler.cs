using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Create;

public sealed class Handler(
    IInspectionRepository inspectionRepository,
    IEmergencyCartRepository emergencyCartRepository,
    IEmployeeRepository employeeRepository,
    IUnitOfWork ofWork) : ICommandHandler<Command, Response>
{
    public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
    {
        var validationResult = Validate(request);
        if (validationResult.IsFailure)
            return Result.Failure<Response>(validationResult.Error);

        var emergencyCart = await emergencyCartRepository.GetByIdWithItemsAsync(request.EmergencyCartId, cancellationToken);
        if (emergencyCart is null)
            return Result.Failure<Response>(Error.NotFound("404", "Emergency cart not found"));

        var employee = await employeeRepository.GetUserAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result.Failure<Response>(Error.NotFound("404", "Employee not found"));

        if (!emergencyCart.IsActive)
            return Result.Failure<Response>(Error.BadRequest("400", "Emergency cart is inactive"));

        if (await inspectionRepository.ExistsInProgressInspectionAsync(emergencyCart.Id, cancellationToken))
            return Result.Failure<Response>(Error.Conflict("409", "There is already an in-progress inspection for this emergency cart"));

        if (emergencyCart.CartItems.Count == 0)
            return Result.Failure<Response>(Error.BadRequest("400", "Emergency cart lacks items for inspection."));

        var inspection = Inspection.Create(emergencyCart.Id, employee.Id);

        foreach (var cartItem in emergencyCart.CartItems)
        {
            inspection.AddItem(cartItem.ItemId, cartItem.ExpectedQuantity);
        }

        await inspectionRepository.AddAsync(inspection, cancellationToken);
        await ofWork.CommitAsync(cancellationToken);

        return Result.Success(new Response(inspection.Id, "Inspection created successfully"));
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
