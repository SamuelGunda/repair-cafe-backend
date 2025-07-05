using MediatR;
using Microsoft.AspNetCore.Mvc;
using RepairCafe.Shared.Kernel.Abstractions;
using RepairCafe.Shared.Kernel.Entities;

namespace RepairCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

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
            _ => BadRequest(result.Error.Description)
        };
    }
}
