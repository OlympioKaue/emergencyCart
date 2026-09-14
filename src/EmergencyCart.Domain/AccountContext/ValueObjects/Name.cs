using EmergencyCart.Domain.AccountContext.Enums;
using EmergencyCart.Domain.SharedContext.ValueObjects;
using System.Text.RegularExpressions;

namespace EmergencyCart.Domain.AccountContext.ValueObjects;

public sealed partial record class Name : ValueObject
{
    public static readonly HashSet<string> Prepositions = new(StringComparer.OrdinalIgnoreCase)
    {
        "da", "de", "do", "das", "dos", "e"
    };
    public const string Pattern = @"^\p{L}+(?: \p{L}+)*$";

    private Name() { }

    private Name(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public string FirstName { get; } = string.Empty;
    public string LastName { get; } = string.Empty;

    private static void Validate(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new Exception();

        if (!NameRegex().IsMatch(firstName) || !NameRegex().IsMatch(lastName))
            throw new Exception();
    }

    public static Name Create(string firstName, string lastName)
    {
        Validate(firstName, lastName);

        var formattedFirstName = FormatName(firstName);
        var formattedLastName = FormatName(lastName);

        return new Name(formattedFirstName, formattedLastName);
    }

    public static string FormatName(string name)
    {
        var words = name
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var formattedWords = words.Select((word, index) => FormatWord(word, isFirstWord: index == 0));

        return string.Join(' ', formattedWords);
    }

    private static string FormatWord(string word, bool isFirstWord)
    {
        var lower = word.ToLowerInvariant();

        if (!isFirstWord && Prepositions.Contains(lower))
            return lower;

        return char.ToUpperInvariant(lower[0]) + lower[1..];
    }

    public static string CreateUseCode(string firstName, string lastName, EmployeeRole role)
    {
        var parts = SplitNameIgnoringPrepositions(firstName, lastName);

        if (parts.Length == 0)
            throw new Exception("trata aqui");

        var namePart = parts.Length == 1
       ? parts[0].ToUpperInvariant()
       : BuildInitialsPlusLastName(parts);

        return $"{role.ToString().ToUpperInvariant()}-{namePart}";
    }

    private static string[] SplitNameIgnoringPrepositions(string firstName, string lastName)
    {
        return $"{firstName} {lastName}"
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => !Prepositions.Contains(p))
            .ToArray();
    }

    private static string BuildInitialsPlusLastName(string[] parts)
    {
        var initials = string.Concat(parts.Take(parts.Length - 1).Select(p => p[0]));
        var lastName = parts[^1];

        return $"{initials}{lastName}".ToUpperInvariant();
    }


    [GeneratedRegex(Pattern)]
    public static partial Regex NameRegex();
}
