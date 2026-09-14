using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;
using EmergencyCart.Infrastructure.SharedContext.Data;
using Microsoft.EntityFrameworkCore;

namespace EmergencyCart.Infrastructure.AccountContext.Repositories.Abstractions;

internal class EmployeeRepository(AppDbContext dbContext) : IEmployeeRepository
{
    public async Task AddUserAsync(Employee user, CancellationToken cancellationToken)
    => await dbContext.Employees.AddAsync(user, cancellationToken);

    public void Delete(Employee user)
    {
        dbContext.Employees.Remove(user);
    }

    public async Task<Employee?> GetUserAsync(Guid id, CancellationToken cancellationToken)
    => await dbContext.Employees.FirstOrDefaultAsync(y => y.Id == id, cancellationToken);

    public async Task<Employee?> GetUserEmailAsync(string email, CancellationToken cancellationToken)
    => await dbContext.Employees.FirstOrDefaultAsync(y => y.Email.Address == email, cancellationToken);

    public void Update(Employee user)
    {
        dbContext.Employees.Update(user);
    }

    public async Task<bool> VerifyEmailExistsAsync(string email, CancellationToken cancellationToken)
    => await dbContext.Employees.AsNoTracking().AnyAsync(y => y.Email.Address == email, cancellationToken);

    public async Task<List<Employee>> GetEmployeesAsync(CancellationToken cancellationToken)
    => await dbContext.Employees.AsNoTracking().ToListAsync(cancellationToken);

}
