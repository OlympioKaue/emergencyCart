using EmergencyCart.Domain.AccountContext.Enums;
using EmergencyCart.Domain.AccountContext.ValueObjects;
using EmergencyCart.Domain.SharedContext.AggregateRoots.Abstractions;
using EmergencyCart.Domain.SharedContext.Entities;
using System.ComponentModel.DataAnnotations;

namespace EmergencyCart.Domain.AccountContext.Entities;

public sealed class Employee : Entity, IAggregateRoots
{
    private Employee() : base(Guid.CreateVersion7()) { }

    private Employee(Guid id, Name name, Email email, Password password, EmployeeRole role, string userCode) : base(id)
    {

        Name = name;
        Email = email;
        Password = password;
        Roles = role;
        UserCode = userCode;
        IsActive = true;
    }


    public Name Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Password Password { get; private set; } = null!;
    public string UserCode { get; private set; } = null!;
    public EmployeeRole Roles { get; private set; }
    public bool IsActive { get; } = false;

    public static Employee Create(Name name, Email email, Password password, EmployeeRole role)
    {
        var employeeCode = Name.CreateUseCode(name.FirstName, name.LastName, role);
        if (employeeCode is null)
            throw new ValidationException("Unexpected error: The generated code is invalid.");
        return new Employee(Guid.CreateVersion7(), name, email, password, role, employeeCode);
    }

    public void ChangeNameUpdate(string? firstName, string? lastName)
    {
        var newFirstName = string.IsNullOrWhiteSpace(firstName) ? Name.FirstName : firstName;
        var newLastName = string.IsNullOrWhiteSpace(lastName) ? Name.LastName : lastName;

        Name = Name.Create(newFirstName, newLastName);
    }

    public void ChangeEmailUpdate(string? email)
    {
        var newFirstEmail = string.IsNullOrWhiteSpace(email) ? Email.Address : email;

        Email = Email.Create(newFirstEmail);
    }

    public bool Challenge(string password)
    => Password.Verify(password, this.Password.Hash);

    public void ChangePasswordEmail(string password)
    {
        Password = Password.Create(password);
    }
}
