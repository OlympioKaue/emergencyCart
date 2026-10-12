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
    public class InspectionController : ControllerBase
    {
        [Authorize(Roles = "Pharmacist,Nurse")]
        [HttpPost("{emergencyCartId:guid}")]
        [ProducesResponseType(typeof(EmergencyCart.Application.AccountContext.UseCases.Inspections.Create.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(
            [FromServices] ISender sender,
            [FromRoute] Guid emergencyCartId)
        {
            var employeeIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(employeeIdClaim, out var employeeId))
                return Unauthorized();

            var result = await sender.Send(new EmergencyCart.Application.AccountContext.UseCases.Inspections.Create.Command(emergencyCartId, employeeId));
            if (result.IsFailure)
            {
                return result.Error.type switch
                {
                    ErrorType.NotFound => NotFound(ErrorResponse.From(result.Error)),
                    ErrorType.Conflict => Conflict(ErrorResponse.From(result.Error)),
                    _ => BadRequest(ErrorResponse.From(result.Error))
                };
            }
            return Created($"api/inspection/{result.Value.id}", result.Value);
        }

        [Authorize(Roles = "Pharmacist,Nurse")]
        [HttpPatch("{inspectionId:guid}/items/{itemId:guid}")]
        [ProducesResponseType(typeof(EmergencyCart.Application.AccountContext.UseCases.Inspections.RegisterItem.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]

        public async Task<IActionResult> RegisterItem(
         [FromServices] ISender sender,
         [FromRoute] Guid inspectionId,
         [FromRoute] Guid itemId,
         [FromBody] EmergencyCart.Application.AccountContext.UseCases.Inspections.RegisterItem.Command request)
        {
            var employeeIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(employeeIdClaim, out var employeeId))
                return Unauthorized();

            var result = await sender.Send(new EmergencyCart.Application.AccountContext.UseCases.Inspections.RegisterItem.Command(inspectionId, itemId, employeeId, request.FoundQuantity));
            if (result.IsFailure)
            {
                return result.Error.type switch
                {
                    ErrorType.NotFound => NotFound(ErrorResponse.From(result.Error)),
                    ErrorType.Conflict => Conflict(ErrorResponse.From(result.Error)),
                    ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.From(result.Error)),
                    _ => BadRequest(ErrorResponse.From(result.Error))
                };
            }

            return Ok(result.Value);
        }

        [Authorize(Roles = "Pharmacist,Nurse")] 
        [HttpPatch("{inspectionId:guid}/complete")]
        [ProducesResponseType(typeof(EmergencyCart.Application.AccountContext.UseCases.Inspections.Complete.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]

        public async Task<IActionResult> Complete(
        [FromServices] ISender sender,
        [FromRoute] Guid inspectionId)
        {
            var employeeIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(employeeIdClaim, out var employeeId))
                return Unauthorized();

            var result = await sender.Send(new EmergencyCart.Application.AccountContext.UseCases.Inspections.Complete.Command(inspectionId, employeeId));
            if (result.IsFailure)
            {
                return result.Error.type switch
                {
                    ErrorType.NotFound => NotFound(ErrorResponse.From(result.Error)),
                    ErrorType.Conflict => Conflict(ErrorResponse.From(result.Error)),
                    ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.From(result.Error)),
                    _ => BadRequest(ErrorResponse.From(result.Error))
                };
            }

            return Ok(result.Value);
        }

        [Authorize(Roles = "Pharmacist,Nurse")]  
        [HttpGet("{inspectionId:guid}/comparison")]
        [ProducesResponseType(typeof(EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]

        public async Task<IActionResult> GetComparison(
        [FromServices] ISender sender,
        [FromRoute] Guid inspectionId,
        [FromQuery] bool onlyMissing = false)
        {
            var result = await sender.Send(new EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.Query(inspectionId, onlyMissing));
            if (result.IsFailure)
            {
                return result.Error.type switch
                {
                    ErrorType.NotFound => NotFound(ErrorResponse.From(result.Error)),
                    _ => BadRequest(ErrorResponse.From(result.Error))
                };
            }

            return Ok(result.Value);
        }
    }
}
