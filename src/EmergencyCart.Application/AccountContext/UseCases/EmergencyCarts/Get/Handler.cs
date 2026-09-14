using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.EmergencyCarts.Get;

public sealed class Handler(IEmergencyCartRepository emergencyCartRepository) : IQueryHandler<Query, IReadOnlyList<Response>>
{
    public async Task<Result<IReadOnlyList<Response>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var cart = await emergencyCartRepository.GetAll(cancellationToken);
        var response = cart.Select(i => new Response(i.Id, i.Code, i.SectorId, i.Sector.Names, i.CartItems.Count)).ToList();

        return Result.Success<IReadOnlyList<Response>>(response);
    }
}
