using EmergencyCart.Domain.AccountContext.Entities;

namespace EmergencyCart.Application.AccountContext.Repositories.Abstractions.Security;

public interface ITokenGenerator
{
    public string GenerateToken(Employee employee);
}
