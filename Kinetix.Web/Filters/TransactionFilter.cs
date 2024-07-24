using Kinetix.Services;
using Microsoft.AspNetCore.Http;

namespace Kinetix.Web.Filters;

/// <summary>
/// Filtre pour gérer la transaction.
/// </summary>
public class TransactionFilter(TransactionScopeManager transactionScopeManager) : IEndpointFilter
{
    private ServiceScope? _scope;

    /// <inheritdoc cref="IEndpointFilter.InvokeAsync" />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        _scope = transactionScopeManager.EnsureTransaction();
        try
        {
            var result = await next(context);
            _scope?.Complete();
            return result;
        }
        finally
        {
            _scope?.Dispose();
        }
    }
}
