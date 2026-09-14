using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Items.Get;

public sealed class Handler(IItemRepository itemRepository) : IQueryHandler<Query, IReadOnlyList<Response>>
{
    public async Task<Result<IReadOnlyList<Response>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var item = await itemRepository.GetAllAsync(cancellationToken);
        if (item is null)
            return Result.Failure<IReadOnlyList<Response>>(Error.NotFound("404", "Item not found"));

        var response = item.Select(i => new Response(i.Id, i.Names, i.Category)).ToList();

        return Result.Success<IReadOnlyList<Response>>(response);
    }
}
