using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;

namespace EmergencyCart.Application.AccountContext.Repositories.Abstractions;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task AddUserAsync(Employee employee, CancellationToken cancellationToken);
    Task<bool> VerifyEmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<Employee?> GetUserAsync(Guid id, CancellationToken cancellationToken);
    Task<Employee?> GetUserEmailAsync(string email, CancellationToken cancellationToken);
    Task<List<Employee>> GetEmployeesAsync(CancellationToken cancellationToken);
    void Update(Employee employee);
    void Delete(Employee employee);
}
