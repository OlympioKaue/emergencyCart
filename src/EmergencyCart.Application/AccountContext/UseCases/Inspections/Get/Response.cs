using EmergencyCart.Application.SharedContext.UseCases.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;
using EmergencyCart.Domain.AccountContext.Enums;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get;

public sealed record class Response(
    Guid InspectionId, 
    InspectionStatus Status, 
    IReadOnlyCollection<ItemResponse> Items) : IQueryResponse;
