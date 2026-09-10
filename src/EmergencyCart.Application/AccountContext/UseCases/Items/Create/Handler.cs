using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;
using EmergencyCart.Domain.AccountContext.Enums;

namespace EmergencyCart.Application.AccountContext.UseCases.Items.Create;

public sealed class Handler(IItemRepository itemRepository, IUnitOfWork ofWork) : ICommandHandler<Command, Response>
{
    public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
    {
        var validationResult = Validate(request);
        if (validationResult.IsFailure)
            return Result.Failure<Response>(validationResult.Error);

        var category = Enum.Parse<ItemCategory>(request.category, ignoreCase: true);

        var itemExists = await itemRepository.ExistsItemAsync(request.name, category, cancellationToken);
        if (itemExists)
            return Result.Failure<Response>(Error.Conflict("409", "Item already exists"));

        var item = Item.Create(request.name, category);

        await itemRepository.AddAsync(item, cancellationToken);
        await ofWork.CommitAsync(cancellationToken);

        return Result.Success(new Response("Item created successfully"));
    }

    private static Result Validate(Command command)
    {
        var validation = new Validator();
        var result = validation.Validate(command);

        if (result.IsValid)
            return Result.Success();

        var errors = result.Errors.Select(e => e.ErrorMessage).ToArray();
        return Result.Failure<Response>(new ValidationError(errors));
    }
}
