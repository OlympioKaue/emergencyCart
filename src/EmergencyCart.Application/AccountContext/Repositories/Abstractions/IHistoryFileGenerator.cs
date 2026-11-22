using EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetHistory;
using EmergencyCart.Domain.AccountContext.Enums;

namespace EmergencyCart.Application.AccountContext.Repositories.Abstractions;

public interface IHistoryFileGenerator
{
    byte[] Generate(ExportFormat format, IReadOnlyCollection<HistoryItemResponse> items, string filterLabel, DateTime generatedAtUtc);
}