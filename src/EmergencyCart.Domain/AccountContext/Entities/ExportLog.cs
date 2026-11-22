using EmergencyCart.Domain.AccountContext.Enums;
using EmergencyCart.Domain.SharedContext.AggregateRoots.Abstractions;
using EmergencyCart.Domain.SharedContext.Entities;
using System.ComponentModel.DataAnnotations;

namespace EmergencyCart.Domain.AccountContext.Entities;

public sealed class ExportLog : Entity, IAggregateRoots
{
    private ExportLog() : base(Guid.CreateVersion7()) { }

    private ExportLog(Guid id, Guid employeeId, ExportFormat format, Guid? emergencyCartId, int recordCount) : base(id)
    {
        EmployeeId = employeeId;
        Format = format;
        EmergencyCartId = emergencyCartId;
        RecordCount = recordCount;
        ExportedAt = DateTime.UtcNow;
    }

    public Guid EmployeeId { get; }
    public ExportFormat Format { get; }
    public Guid? EmergencyCartId { get; }
    public int RecordCount { get; }
    public DateTime ExportedAt { get; }
    public Employee Employee { get; } = null!;

    public static ExportLog Create(Guid employeeId, ExportFormat format, Guid? emergencyCartId, int recordCount)
    {
        Validate(employeeId, format, emergencyCartId, recordCount);
        return new ExportLog(Guid.CreateVersion7(), employeeId, format, emergencyCartId, recordCount);
    }

    private static void Validate(Guid employeeId, ExportFormat format, Guid? emergencyCartId, int recordCount)
    {
        if (employeeId == Guid.Empty)
            throw new ValidationException("Unexpected error: Invalid employee ID.");

        if (!Enum.IsDefined(format))
            throw new ValidationException("Unexpected error: Invalid export format.");

        if (emergencyCartId == Guid.Empty)
            throw new ValidationException("Unexpected error: Invalid emergency cart ID.");

        if (recordCount <= 0)
            throw new ValidationException("Unexpected error: Invalid record count.");
    }
}