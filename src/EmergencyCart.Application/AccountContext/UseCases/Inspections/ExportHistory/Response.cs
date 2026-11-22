using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.ExportHistory;

public sealed record class Response(byte[] Content, string ContentType, string FileName) : ICommandResponse;