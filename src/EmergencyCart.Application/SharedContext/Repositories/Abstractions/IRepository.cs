using EmergencyCart.Domain.SharedContext.AggregateRoots.Abstractions;

namespace EmergencyCart.Application.SharedContext.Repositories.Abstractions;

public interface IRepository<TAggregate> where TAggregate : IAggregateRoots;
