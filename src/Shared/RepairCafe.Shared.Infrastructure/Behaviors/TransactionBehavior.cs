using RepairCafe.Shared.Kernel.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace RepairCafe.Shared.Infrastructure.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly DbContext _dbContext;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(DbContext dbContext, IDomainEventDispatcher domainEventDispatcher, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _dbContext = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (typeof(TRequest).Name.EndsWith("Query"))
        {
            return await next(cancellationToken);
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var response = await next(cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await _domainEventDispatcher.DispatchEventsAsync(_dbContext, cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Transaction committed for {Request}", typeof(TRequest).Name);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed for {Request}", typeof(TRequest).Name);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}