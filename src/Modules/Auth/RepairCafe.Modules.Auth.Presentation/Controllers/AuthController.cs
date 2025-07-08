using MediatR;
using Microsoft.AspNetCore.Mvc;
using RepairCafe.Modules.Auth.Application.Commands;
using RepairCafe.Shared.Presentation.Controllers;

namespace Repaircafe.Modules.Auth.Presentation.Controllers;

public class AuthController : ApiControllerBase
{
    public AuthController(ISender mediator) : base(mediator)
    {
        
    }

    
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var result = await Mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(Register), result.Value);
        }
        
        return BadRequest(result.Error);
    }
}