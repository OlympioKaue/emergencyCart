using EmergencyCart.Application.AccountContext.UseCases.Employees.Create;

namespace EmergencyCart.UniTest.AccountContext;

public sealed class CommandUser
{
    public static Command Build()
    => new("Ronaldo", "De Souza", "ronaldosouza@gmail.com", "User", "semSenha@");
}
