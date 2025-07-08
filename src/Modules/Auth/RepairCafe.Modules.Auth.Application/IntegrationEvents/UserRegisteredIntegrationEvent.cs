using RepairCafe.Shared.Application.Abstractions;

namespace RepairCafe.Modules.Auth.Application.IntegrationEvents;

public record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Email,
    string UserName) : IIntegrationEvent;
