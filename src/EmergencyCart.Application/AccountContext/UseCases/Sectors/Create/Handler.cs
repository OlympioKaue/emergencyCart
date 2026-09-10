using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;

namespace EmergencyCart.Application.AccountContext.UseCases.Sectors.Create;

public sealed class Handler(ISectorRepository sectorRepository, IUnitOfWork ofWork) : ICommandHandler<Command, Response>
{
    public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
    {
        request = request with { name = Sector.NormalizeSpaces(request.name) };
        var validationResult = Validate(request);
        if (validationResult.IsFailure)
            return Result.Failure<Response>(validationResult.Error);

        var alreadyExists = await sectorRepository.VerifyExistNameSector(request.name, cancellationToken);
        if (alreadyExists)
            return Result.Failure<Response>(Error.Conflict("409", "A sector with the same name already exists."));

        var sector = Sector.Create(request.name);

        await sectorRepository.AddAsync(sector, cancellationToken);
        await ofWork.CommitAsync(cancellationToken);

        return Result.Success(new Response("Sector created successfully", sector.Id));
    }

    private static Result Validate(Command command)
    {
        var validation = new Validator();
        var result = validation.Validate(command);

        if (result.IsValid)
            return Result.Success();

        var errors = result.Errors.Select(e => e.ErrorMessage).ToArray();
        return Result.Failure<Response>(new ValidationError(errors));
    }
}
