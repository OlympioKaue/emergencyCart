using EmergencyCart.Domain.AccountContext.Enums;
using EmergencyCart.Domain.SharedContext.AggregateRoots.Abstractions;
using EmergencyCart.Domain.SharedContext.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EmergencyCart.Domain.AccountContext.Entities;

public sealed partial class Inspection : Entity, IAggregateRoots
{
    private readonly List<InspectionItem> _inspectionItems = [];
    private Inspection() : base(Guid.CreateVersion7()) { }

    private Inspection(Guid id, Guid emergencyCartId, Guid employeeId) : base(id)
    {
        EmergencyCartId = emergencyCartId;
        EmployeeId = employeeId;
        StartedAt = DateTime.UtcNow;
        Status = InspectionStatus.InProgress;
    }

    public Guid EmergencyCartId { get; }
    public Guid EmployeeId { get; }
    public DateTime StartedAt { get; }
    public DateTime? FinishedAt { get; private set; }
    public InspectionStatus Status { get; private set; }

    public EmergencyCart EmergencyCart { get; } = null!;
    public Employee Employee { get; } = null!;
    public IReadOnlyCollection<InspectionItem> InspectionItems => _inspectionItems.ToArray();

    private static void Validate(Guid emergencyCartId, Guid employeeId)
    {
        if (emergencyCartId == Guid.Empty)
            throw new ArgumentException("Invalid emergency cart ID.", nameof(emergencyCartId));

        if (employeeId == Guid.Empty)
            throw new ArgumentException("Invalid employee ID.", nameof(employeeId));
    }


    public static Inspection Create(Guid emergencyCartId, Guid employeeId)
    {
        Validate(emergencyCartId, employeeId);
        return new Inspection(Guid.CreateVersion7(), emergencyCartId, employeeId);
    }

    public void AddItem(Guid itemId, int expectedQuantity)
    {
        if (Status != InspectionStatus.InProgress)
            throw new ValidationException("Unexpected error: The inspection is not in progress.");

        if (_inspectionItems.Any(x => x.ItemId == itemId))
            throw new ValidationException("Unexpected error: The inspection item already exists.");

        _inspectionItems.Add(InspectionItem.Create(Id, itemId, expectedQuantity));
    }

    public void RegisterFoundQuantity(Guid itemId, int foundQuantity)
    {
        if (Status != InspectionStatus.InProgress)
            throw new ValidationException("Unexpected error: The inspection is not in progress.");

        var inspectionItem = _inspectionItems.FirstOrDefault(x => x.ItemId == itemId);
        if (inspectionItem is null)
            throw new ValidationException("Unexpected error: The inspection item does not exist.");

        inspectionItem.RegisterFoundQuantity(inspectionItem, foundQuantity);
    }

    public void RegisterReplenishment(Guid itemId, Guid employeeId, int quantity)
    {
        if (Status != InspectionStatus.Completed)
            throw new ValidationException("Unexpected error: Only completed inspections can have items replenished.");

        var inspectionItem = _inspectionItems.FirstOrDefault(x => x.ItemId == itemId) ?? throw new ValidationException("Unexpected error: The inspection item does not exist.");
        inspectionItem.RegisterReplenishment(employeeId, quantity);
    }

    public void RegisterNote(Guid itemId, string? note)
    {
        if (Status != InspectionStatus.Completed)
            throw new ValidationException("Unexpected error: Only completed inspections can have notes.");

        var inspectionItem = _inspectionItems.FirstOrDefault(x => x.ItemId == itemId);
        if (inspectionItem is null)
            throw new ValidationException("Unexpected error: The inspection item does not exist.");

        inspectionItem.RegisterNote(note);
    }

    public void Complete()
    {
        if (Status != InspectionStatus.InProgress)
            throw new Exception("Inspection is already completed.");

        Status = InspectionStatus.Completed;
        FinishedAt = DateTime.UtcNow;
    }
}
