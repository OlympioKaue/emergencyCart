namespace EmergencyCart.Domain.AccountContext.Enums;

public enum ComparisonStatus : int
{
    NotChecked = 1,   // ainda não conferido
    Matched = 2,      // encontrado = esperado
    Missing = 3,      // encontrado < esperado
    Surplus = 4       // encontrado > esperado
}
