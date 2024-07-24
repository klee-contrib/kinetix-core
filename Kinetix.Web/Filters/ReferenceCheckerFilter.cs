using Kinetix.Services;
using Microsoft.AspNetCore.Http;

namespace Kinetix.Web.Filters;

/// <summary>
/// Filtre permettant de vérifier les valeurs de listes de références (si on n'utilise pas d'enums).
/// </summary>
public class ReferenceCheckerFilter(IReferenceManager referenceManager) : IEndpointFilter
{
    /// <inheritdoc cref="IEndpointFilter.InvokeAsync" />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var parameter in context.Arguments)
        {
            referenceManager.CheckReferenceKeys(parameter);
        }

        return await next(context);
    }
}
