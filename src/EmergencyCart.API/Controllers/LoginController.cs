using EmergencyCart.Application.AccountContext.UseCases.DoLogins;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.Results.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmergencyCart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Login([FromServices] ISender sender, [FromBody] Command request)
        {
            var result = await sender.Send(request);

            if (result.IsFailure)
            {
                return result.Error.type switch
                {
                    ErrorType.Unauthorized => Unauthorized(ErrorResponse.From(result.Error)),
                    ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.From(result.Error)),
                    _ => BadRequest(ErrorResponse.From(result.Error))
                };
            }

            return Ok(result.Value);
        }
    }
}
