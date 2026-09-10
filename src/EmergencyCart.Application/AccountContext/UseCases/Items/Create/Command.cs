using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Items.Create;

public sealed record Command(string name, string category) : ICommand<Response>
{
}
