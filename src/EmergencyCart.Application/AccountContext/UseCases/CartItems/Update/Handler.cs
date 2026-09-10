using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.CartItems.Update;

public sealed class Handler(ICartItemRepository cartItemRepository, IUnitOfWork ofWork) : ICommandHandler<Command, Response>
{
    public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
    {
        var validationResult = Validate(request);
        if (validationResult.IsFailure)
            return Result.Failure<Response>(validationResult.Error);

        var cartItem = await cartItemRepository.GetCartItemByIdAsync(request.cartItemId, cancellationToken);
        if (cartItem is null)
            return Result.Failure<Response>(Error.NotFound("404", "Cart item not found."));

        cartItem.IncreaseQuantity(request.quantity);

        cartItemRepository.Update(cartItem);
        await ofWork.CommitAsync(cancellationToken);

        return Result.Success(new Response());
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

