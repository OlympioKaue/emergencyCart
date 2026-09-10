using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.Results.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmergencyCart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmergencyCartController : ControllerBase
    {
        //estou aqui
        //ja vimos, sector, item, agora estamos aqui, falta (cartItem e user).
        [ProducesResponseType(typeof(EmergencyCart.Application.AccountContext.UseCases.EmergencyCarts.Create.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [HttpPost("{sectorId:guid}")]
        public async Task<IActionResult> Create
            ([FromServices] ISender sender,
            [FromBody] EmergencyCart.Application.AccountContext.UseCases.EmergencyCarts.Create.Command request,
            [FromRoute] Guid sectorId)
        {
            var result = await sender.Send(request with { sectorId = sectorId });
            if (result.IsFailure)
            {
                return result.Error.type switch
                {
                    ErrorType.NotFound => NotFound(ErrorResponse.From(result.Error)),
                    ErrorType.Conflict => Conflict(ErrorResponse.From(result.Error)),
                    _ => BadRequest(ErrorResponse.From(result.Error))
                };
            }

            return Created("", result.Value.message);
        }
    }
}