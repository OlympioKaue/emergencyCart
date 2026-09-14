using EmergencyCart.Domain.SharedContext.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EmergencyCart.Domain.AccountContext.ValueObjects;

public sealed partial record class Email : ValueObject
{
    public const string Pattern = @"^[a-zA-Z0-9._%+-]{3,}@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    public const int MinLength = 3;

    private Email() { }

    private Email(string email)
    {
        Address = email;
    }

    public string Address { get; } = string.Empty;

    private static void Validate(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !EmailRegex().IsMatch(email))
            throw new ValidationException("Unexpected error: The generated email does not meet valid requirements.");

        if (email.Split('@')[0].Length < MinLength)
            throw new ValidationException("Unexpected error: The generated email does not meet valid requirements.");
    }

    public static Email Create(string email)
    {
        Validate(email);

        var formattedEmail = email.ToLower();

       return new Email(formattedEmail);
    }


    [GeneratedRegex(Pattern)]
    private static partial Regex EmailRegex();

}
