using EmergencyCart.Domain.AccountContext.ValueObjects;
using EmergencyCart.Domain.SharedContext.AggregateRoots.Abstractions;
using EmergencyCart.Domain.SharedContext.Entities;
using System.Text.RegularExpressions;

namespace EmergencyCart.Domain.AccountContext.Entities;

public sealed partial class Sector : Entity, IAggregateRoots
{
    public const string Pattern = @"^(?=.*\s)[\p{L}\s]+$";
    public const string Patterntwo = @"^SEC-[0-9A-F]{8}$";
    private readonly List<EmergencyCart> _emergencyCarts;

    private Sector() : base(Guid.CreateVersion7()) { }
    private Sector(Guid id, string name, string code) : base(id)
    {
        _emergencyCarts = [];
        Names = name;
        Code = code;
        IsActive = true;
    }
    public string Names { get; }
    public string Code { get; } = string.Empty;
    public bool IsActive { get; } = false;
    public IReadOnlyCollection<EmergencyCart> EmergencyCarts => _emergencyCarts.ToArray();

    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("Nome do setor não pode ser vazio.");

        if (name.Trim().Length < 3)
            throw new Exception("Nome do setor muito curto.");

        if (!NameRegex().IsMatch(name))
            throw new Exception("Nome do setor contém caracteres inválidos.");
    }
    public static Sector Create(string name)
    {
        Validate(name);
        var id = Guid.CreateVersion7();
        var formattedName = Name.FormatName(name);
        var code = GenerateFrom(id);
        return new Sector(id, formattedName, code);
    }

    public static string NormalizeSpaces(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return string.Join(' ', name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string GenerateFrom(Guid sectorId)
    {
        var shortCode = sectorId.ToString("N")[..8].ToUpperInvariant();
        return $"SEC-{shortCode}";
    }

    [GeneratedRegex(Pattern)]
    private static partial Regex NameRegex();
}