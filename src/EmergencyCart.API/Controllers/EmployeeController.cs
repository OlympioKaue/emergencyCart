using EmergencyCart.Application.AccountContext.UseCases.Employees.GetAll;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.Results.Enums;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmergencyCart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(Application.AccountContext.UseCases.Employees.Create.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create
            ([FromServices] ISender sender,
            [FromBody] Application.AccountContext.UseCases.Employees.Create.Command request)
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

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutById
            ([FromServices] ISender sender,
            [FromBody] Application.AccountContext.UseCases.Employees.Update.Command request,
            [FromRoute] Guid id)
        {
            var result = await sender.Send(request with { id = id });

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

        [Authorize(Roles = "Admin")]
        [HttpGet("employees")]
        [ProducesResponseType(typeof(Application.AccountContext.UseCases.Employees.GetAll.Response), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromServices] ISender sender)
        {
            var result = await sender.Send(new Query());
            return Ok(result.Value);
        }



        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Application.AccountContext.UseCases.Employees.GetById.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromServices] ISender sender, [FromRoute] Guid id)
        {
            var result = await sender.Send(new EmergencyCart.Application.AccountContext.UseCases.Employees.GetById.Query(id));

            if (result.IsFailure)
                return NotFound(ErrorResponse.From(result.Error));

            return Ok(result.Value);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteById
         ([FromServices] ISender sender, [FromRoute] Guid id)
        {
            var result = await sender.Send(new EmergencyCart.Application.AccountContext.UseCases.Employees.Delete.Command(id));

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
