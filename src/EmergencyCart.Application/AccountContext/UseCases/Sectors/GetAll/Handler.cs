using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.Results.Enums;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Sectors.GetAll;

public sealed class Handler(ISectorRepository sectorRepository) : IQueryHandler<Query, IReadOnlyList<Response>>
{
    public async Task<Result<IReadOnlyList<Response>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var sector = await sectorRepository.GetAllAsync(cancellationToken);

        var response = sector.Select(i => new Response(i.Id, i.Code, i.Names, i.IsActive)).ToList();

        return Result.Success<IReadOnlyList<Response>>(response);
    }
}
