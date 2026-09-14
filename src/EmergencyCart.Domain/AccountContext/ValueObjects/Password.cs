using EmergencyCart.Domain.SharedContext.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EmergencyCart.Domain.AccountContext.ValueObjects;

public sealed partial record class Password : ValueObject
{
    public const string Pattern = "^(?=.*[A-Z])(?=.*[\\W_]).{7,}$";

    private Password() { }

    private Password(string password)
    {
        Hash = password;
    }


    public string Hash { get; } = string.Empty;


    public static void Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ValidationException("Unexpected error: The generated password does not meet the valid requirements.");

        if (!HashPasswordRegex().IsMatch(password))
            throw new ValidationException("Unexpected error: The generated password does not meet the valid requirements.");
    }


    public static Password Create(string password)
    {
        Validate(password);

        var passwordHash = Encrypt(password);

        return new Password(passwordHash);
    }

    private static string Encrypt(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public static bool Verify(string password, string hashPassword)
        => BCrypt.Net.BCrypt.Verify(password, hashPassword);


    [GeneratedRegex(Pattern)]
    public static partial Regex HashPasswordRegex();

}
