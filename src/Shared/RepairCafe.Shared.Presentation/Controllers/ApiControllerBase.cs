using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepairCafe.Shared.Kernel.Entities;

namespace RepairCafe.Shared.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected readonly ISender Mediator;

    protected ApiControllerBase(ISender mediator)
    {
        Mediator = mediator;
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error.Type switch
        {
            ErrorType.Validation => BadRequest(result.Error.Description),
            ErrorType.NotFound => NotFound(result.Error.Description),
            ErrorType.Conflict => Conflict(result.Error.Description),
            ErrorType.Failure => StatusCode(StatusCodes.Status500InternalServerError, result.Error.Description),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };
    }
}