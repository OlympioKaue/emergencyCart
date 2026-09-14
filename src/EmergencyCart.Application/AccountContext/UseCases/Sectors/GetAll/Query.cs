using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Sectors.GetAll
{
    public sealed record class Query() : IQuery<IReadOnlyList<Response>>;
    
}
