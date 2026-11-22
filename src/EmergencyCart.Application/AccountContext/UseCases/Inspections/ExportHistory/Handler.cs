using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetHistory;
using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;
using EmergencyCart.Domain.AccountContext.Enums;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.ExportHistory;

public sealed class Handler(
    IInspectionRepository inspectionRepository,
    IExportLogRepository exportLogRepository,
    IHistoryFileGenerator fileGenerator,
    IUnitOfWork unitOfWork) : ICommandHandler<Command, Response>
{
    private const int ExportLimit = 1000;

    public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
    {
        var validationResult = Validate(request);
        if (validationResult.IsFailure)
            return Result.Failure<Response>(validationResult.Error);

        var inspections = await inspectionRepository.GetCompletedForExportAsync(request.EmergencyCartId, ExportLimit + 1, cancellationToken);

        if (inspections.Count == 0)
            return Result.Failure<Response>(Error.NotFound("404", "No completed inspections found to export"));

        if (inspections.Count > ExportLimit)
            return Result.Failure<Response>(Error.BadRequest("400", $"Too many records to export (limit {ExportLimit}). Filter by cart."));

        var items = inspections
            .Select(i => new HistoryItemResponse(
                i.Id,
                i.EmergencyCartId,
                i.EmergencyCart.Code,
                i.EmergencyCart.Sector.Names,
                $"{i.Employee.Name.FirstName} {i.Employee.Name.LastName}",
                DateTime.SpecifyKind(i.FinishedAt!.Value, DateTimeKind.Utc),
                i.InspectionItems.Count,
                i.InspectionItems.Count(x => x.Comparison == ComparisonStatus.Missing)
            )).ToArray();

        var generatedAt = DateTime.UtcNow;
        var filterLabel = request.EmergencyCartId is null
            ? "Todos os carrinhos"
            : $"Carrinho {items[0].CartCode}";

        var content = fileGenerator.Generate(request.Format, items, filterLabel, generatedAt);

        var log = ExportLog.Create(request.EmployeeId, request.Format, request.EmergencyCartId, items.Length);
        await exportLogRepository.AddAsync(log, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var (contentType, extension) = request.Format == ExportFormat.Pdf
            ? ("application/pdf", "pdf")
            : ("text/csv; charset=utf-8", "csv");

        var fileName = $"historico-conferencias-{generatedAt:yyyy-MM-dd}.{extension}";

        return Result.Success(new Response(content, contentType, fileName));
    }

    private static Result Validate(Command request)
    {
        var validation = new Validator();
        var result = validation.Validate(request);
        if (result.IsValid)
            return Result.Success();

        var errors = result.Errors.Select(e => e.ErrorMessage).ToArray();
        return Result.Failure<Response>(new ValidationError(errors));
    }
}