using EmergencyCart.Application.SharedContext.Results.Enums;

namespace EmergencyCart.Application.SharedContext.Results;

public sealed record class ValidationError(string[] Messages) : Error("Error.Validation", "Um ou mais erros de validação ocorreram", ErrorType.BadRequest);

