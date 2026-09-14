using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.Results.Enums;

public sealed class ValidationException : Exception
{
    public ValidationException(IEnumerable<ValidationError> errors)
    => Errors = errors;
     
    public IEnumerable<ValidationError> Errors { get; }
}

//public sealed record class ValidationError(string[] Messages) : Error("Error.Validation", "Um ou mais erros de validação ocorreram", ErrorType.BadRequest);