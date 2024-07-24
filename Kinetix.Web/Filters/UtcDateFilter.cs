using Microsoft.AspNetCore.Http;

namespace Kinetix.Web.Filters;

/// <summary>
/// Filtre pour vérifier que les dates sont bien en Kind UTC.
/// </summary>
public class UtcDateFilter : IEndpointFilter
{
    /// <inheritdoc cref="IEndpointFilter.InvokeAsync" />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var parameters = new List<object?>();

        foreach (var parameter in context.Arguments)
        {
            if (parameter is DateTime date)
            {
                parameters.Add(DateTime.SpecifyKind(date, DateTimeKind.Utc));
            }
            else
            {
                parameters.Add(parameter);
            }
        }

        foreach (var parameter in parameters)
        {
            context.Arguments[parameters.IndexOf(parameter)] = parameter;
        }

        return await next(context);
    }
}
