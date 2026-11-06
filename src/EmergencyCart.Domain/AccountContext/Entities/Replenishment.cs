using EmergencyCart.Domain.SharedContext.AggregateRoots.Abstractions;
using EmergencyCart.Domain.SharedContext.Entities;
using System.ComponentModel.DataAnnotations;

namespace EmergencyCart.Domain.AccountContext.Entities;

public sealed class Replenishment : Entity, IAggregateRoots
{
    private Replenishment() : base(Guid.CreateVersion7()) { }

    private Replenishment(Guid id, Guid inspectionItemId, Guid employeeId, int quantity) : base(id)
    {
        InspectionItemId = inspectionItemId;
        EmployeeId = employeeId;
        Quantity = quantity;
        ReplenishedAt = DateTime.UtcNow;
    }

    public Guid InspectionItemId { get; }
    public Guid EmployeeId { get; }
    public int Quantity { get; }
    public DateTime ReplenishedAt { get; }
    public InspectionItem InspectionItem { get; } = null!;
    public Employee Employee { get; } = null!;

    internal static Replenishment Create(Guid inspectionItemId, Guid employeeId, int quantity)
    {
        Validate(inspectionItemId, employeeId, quantity);
        return new Replenishment(Guid.CreateVersion7(), inspectionItemId, employeeId, quantity);
    }

    private static void Validate(Guid inspectionItemId, Guid employeeId, int quantity)
    {
        if (inspectionItemId == Guid.Empty)
            throw new ValidationException("Unexpected error: Invalid inspection item ID.");

        if (employeeId == Guid.Empty)
            throw new ValidationException("Unexpected error: Invalid employee ID.");

        if (quantity <= 0 || quantity > 5)
            throw new ValidationException("Unexpected error: The quantity does not meet system requirements.");
    }
}