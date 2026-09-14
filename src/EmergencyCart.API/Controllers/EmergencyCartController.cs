using EmergencyCart.Application.AccountContext.UseCases.EmergencyCarts.Create;
using EmergencyCart.Application.AccountContext.UseCases.EmergencyCarts.Get;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.Results.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmergencyCart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmergencyCartController : ControllerBase
    {
        [Authorize(Roles = "Pharmacist")]
        [HttpPost("{sectorCode}")]
        [ProducesResponseType(typeof(EmergencyCart.Application.AccountContext.UseCases.EmergencyCarts.Create.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create
            ([FromServices] ISender sender,
            [FromRoute] string sectorCode)
        {
            var result = await sender.Send(new Command(sectorCode));
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get([FromServices] ISender sender)
        {
            var result = await sender.Send(new Query());
            if (result.IsFailure)
            {
               return NotFound(ErrorResponse.From(result.Error));

            }
            return Ok(result.Value);
        }
    }
}