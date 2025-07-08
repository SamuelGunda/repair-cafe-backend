using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RepairCafe.Modules.Auth.Application.Commands;
using RepairCafe.Modules.Auth.Application.IntegrationEvents;
using RepairCafe.Modules.Auth.Application.Services;
using RepairCafe.Modules.Auth.Domain.Entities;
using RepairCafe.Shared.Application.Abstractions;
using RepairCafe.Shared.Kernel.Entities;

namespace RepairCafe.Modules.Auth.Application.Handlers.Commands;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, Result<AuthTokens>>
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public RegisterCommandHandler(
        UserManager<User> userManager,
        ITokenService tokenService,
        ILogger<RegisterCommandHandler> logger,
        IIntegrationEventPublisher integrationEventPublisher)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task<Result<AuthTokens>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering new user with email {Email}", request.email);

        var user = User.Create(
            request.firstName,
            request.lastName,
            request.email);

        var identityResult = await _userManager.CreateAsync(user, request.password);

        if (!identityResult.Succeeded)
        {
            var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
            _logger.LogWarning("User registration failed: {Errors}", errors);
            
            return Result.Failure<AuthTokens>(
                new Error("Registration.Failed", "User registration failed. Please check the provided information and try again.", ErrorType.Validation));
        }

        // TODO: Add proper role management
        await _userManager.AddToRoleAsync(user, "Basic");

        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));

        var tokens = _tokenService.GenerateTokens(user, roleClaims);

        user.SetRefreshToken(tokens.RefreshToken, tokens.ExpiresAt);
        await _userManager.UpdateAsync(user);

        await _integrationEventPublisher.PublishAsync(
            new UserRegisteredIntegrationEvent(user.Id, user.Email, user.UserName),
            cancellationToken);

        _logger.LogInformation("User registered successfully with ID {UserId}", user.Id);
        
        return Result.Success(tokens);
    }
}