using EmergencyCart.Application.AccountContext.UseCases.Sectors.GetAll;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.Results.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmergencyCart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectorController : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(EmergencyCart.Application.AccountContext.UseCases.Sectors.Create.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create
            ([FromServices] ISender sender,
            [FromBody] EmergencyCart.Application.AccountContext.UseCases.Sectors.Create.Command request)
        {
            var result = await sender.Send(request);
            if (result.IsFailure)
            {
                return result.Error.type switch
                {
                    ErrorType.Conflict => Conflict(ErrorResponse.From(result.Error)),

                    _ => BadRequest(ErrorResponse.From(result.Error))
                };
            }

            return Created("", $"{result.Value.Id} - {result.Value.message}");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get([FromServices] ISender sender)
        {
            var result = await sender.Send(new Query());
            if (result.IsFailure)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Value);
        }
    }
}
