using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.Results.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmergencyCart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemController : ControllerBase
    {
        //ok
        [ProducesResponseType(typeof(EmergencyCart.Application.AccountContext.UseCases.CartItems.Create.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [HttpPost("{emergencyCartId:guid}")]
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

        //ok
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPut("{cartItemId:guid}")]
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
    }
}
