using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;

namespace EmergencyCart.Application.AccountContext.UseCases.CartItems.Create;

public sealed class Handler(
    IEmergencyCartRepository emergencyCartRepository,
    IItemRepository itemRepository,
    ICartItemRepository cartItemRepository,
    IUnitOfWork ofWork) : ICommandHandler<Command, Response>
{
    public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
    {
        var validationResult = Validate(request);
        if (validationResult.IsFailure)
            return Result.Failure<Response>(validationResult.Error);

        var cart = await emergencyCartRepository.GetByIdAsync(request.emergencyCartId, cancellationToken);
        if (cart is null)
            return Result.Failure<Response>(Error.NotFound("404", "The specified emergency cart was not found."));

        var item = await itemRepository.GetByIdAsync(request.itemId, cancellationToken);
        if (item is null)
            return Result.Failure<Response>(Error.NotFound("404", "The specified item was not found."));

        var alreadyExists = await cartItemRepository.GetCartItemAsync(request.emergencyCartId, request.itemId, cancellationToken);
        if (alreadyExists is not null)
            return Result.Failure<Response>(Error.Conflict("409", "This item is already in the emergency cart."));

        var cartItem = CartItem.Create(cart.Id, item.Id, request.expectedQuantity);

        await cartItemRepository.AddAsync(cartItem, cancellationToken);
        await ofWork.CommitAsync(cancellationToken);

        return Result.Success(new Response("Item added to emergency cart successfully."));
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
