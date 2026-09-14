using EmergencyCart.Application.AccountContext.Repositories.Abstractions.Security;
using EmergencyCart.Domain.AccountContext.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmergencyCart.Infrastructure.AccountContext.Security.Token;

internal sealed class TokenGenerator(uint expirationTimeMinute, string signigKey) : ITokenGenerator
{

    public string GenerateToken(Employee employee)
    {
        var claims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new(ClaimTypes.GivenName, employee.Name.FirstName),
            new(ClaimTypes.Email, employee.Email.Address),
            new(ClaimTypes.Role, employee.Roles.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.AddMinutes(expirationTimeMinute),
            SigningCredentials = new SigningCredentials(SecurityKey(), SecurityAlgorithms.HmacSha256Signature),
            Subject = new ClaimsIdentity(claims)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }

    private SymmetricSecurityKey SecurityKey()
    {
        var key = Encoding.UTF8.GetBytes(signigKey);
        return new SymmetricSecurityKey(key);
    }
}
