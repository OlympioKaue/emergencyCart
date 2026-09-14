namespace EmergencyCart.Application.AccountContext.UseCases.Sectors.GetAll
{
    public record class Response(Guid id, string code, string name, bool IsActive)
    {
    }
}
