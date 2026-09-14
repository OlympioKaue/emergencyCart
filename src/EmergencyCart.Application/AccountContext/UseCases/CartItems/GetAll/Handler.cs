using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.CartItems.GetAll;

public sealed class Handler(ICartItemRepository cartItemRepository) : IQueryHandler<Query, IReadOnlyList<Response>>
{
    public async Task<Result<IReadOnlyList<Response>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var cartItem = await cartItemRepository.GetAllAsync(cancellationToken);

        var response = cartItem.Select(i => new Response(i.Id, i.ItemId, i.Item.Names, i.Item.Category.ToString(), i.ExpectedQuantity)).ToList();

        return Result.Success<IReadOnlyList<Response>>(response);
    }
}
