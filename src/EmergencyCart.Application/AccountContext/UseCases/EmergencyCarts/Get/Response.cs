namespace EmergencyCart.Application.AccountContext.UseCases.EmergencyCarts.Get;

public sealed record class Response(Guid id, string code, Guid sectorId, string sectorName, int itemsCount)
{
}
