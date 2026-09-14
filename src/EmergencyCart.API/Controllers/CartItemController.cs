using EmergencyCart.Application.AccountContext.UseCases.CartItems.GetAll;
using EmergencyCart.Application.AccountContext.UseCases.CartItems.GetById;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.Results.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmergencyCart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemController : ControllerBase
    {
        [Authorize(Roles = "Pharmacist")]
        [HttpPost("{emergencyCartId:guid}")]
        [ProducesResponseType(typeof(EmergencyCart.Application.AccountContext.UseCases.CartItems.Create.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create
            ([FromServices] ISender sender,
            [FromRoute] Guid emergencyCartId,
            [FromBody] EmergencyCart.Application.AccountContext.UseCases.CartItems.Create.Command request)
        {
            var result = await sender.Send(request with { emergencyCartId = emergencyCartId });
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


        [Authorize(Roles = "Pharmacist")]
        [HttpPut("{cartItemId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update
            ([FromServices] ISender sender,
            [FromRoute] Guid cartItemId,
            [FromBody] EmergencyCart.Application.AccountContext.UseCases.CartItems.Update.Command request)
        {
            var result = await sender.Send(request with { cartItemId = cartItemId });
            if (result.IsFailure)
            {
                return result.Error.type switch
                {
                    ErrorType.NotFound => NotFound(ErrorResponse.From(result.Error)),
                    _ => BadRequest(ErrorResponse.From(result.Error))
                };
            }
            return NoContent();
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(EmergencyCart.Application.AccountContext.UseCases.CartItems.GetAll.Response), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromServices] ISender sender)
        {
            var result = await sender.Send(new EmergencyCart.Application.AccountContext.UseCases.CartItems.GetAll.Query());
            return Ok(result.Value);
        }

        [Authorize]
        [HttpGet("{emergencycartId:guid}")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromServices] ISender sender, [FromRoute] Guid emergencycartId)
        {
            var result = await sender.Send(new EmergencyCart.Application.AccountContext.UseCases.CartItems.GetById.Query(emergencycartId));
            if (result.IsFailure)
            {
                return NotFound(ErrorResponse.From(result.Error));
            }

            return Ok(result.Value);
        }
    }
}
