using EmergencyCart.Application.AccountContext.UseCases.CartItems.Update;
using EmergencyCart.Application.AccountContext.UseCases.Items.Create;
using EmergencyCart.Application.AccountContext.UseCases.Items.Get;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.Results.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmergencyCart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(EmergencyCart.Application.AccountContext.UseCases.Items.Create.Response), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(
            [FromServices] ISender sender,
            [FromBody] EmergencyCart.Application.AccountContext.UseCases.Items.Create.Command request)
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
            return Created("", result.Value.message);
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(EmergencyCart.Application.AccountContext.UseCases.Items.Get.Response), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromServices] ISender sender)
        {
            var result = await sender.Send(new Query());
            return Ok(result.Value);
        }
    }
}
