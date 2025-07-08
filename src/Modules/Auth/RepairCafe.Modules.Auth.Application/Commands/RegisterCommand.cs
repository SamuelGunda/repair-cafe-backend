using RepairCafe.Modules.Auth.Domain.Entities;
using RepairCafe.Shared.Application.Abstractions;
using RepairCafe.Shared.Kernel.Entities;

namespace RepairCafe.Modules.Auth.Application.Commands;

public record RegisterCommand(
    string email,
    string password,
    string firstName,
    string lastName) : ICommand<Result<AuthTokens>>;