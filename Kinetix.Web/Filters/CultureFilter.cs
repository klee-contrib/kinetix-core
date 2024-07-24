using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Kinetix.Web.Filters;

/// <summary>
/// Filtre pour gérer la culture du header.
/// </summary>
public class CultureFilter : IEndpointFilter
{
    /// <summary>
    /// Nom de l'entête HTTP contenant le code de la culture.
    /// </summary>
    private const string CultureHeaderCode = "CultureCode";

    /// <summary>
    /// Code de la culture par défaut.
    /// </summary>
    private const string DefaultCultureCode = "fr-FR";

    /// <inheritdoc cref="IEndpointFilter.InvokeAsync" />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        context.HttpContext.Request.Headers.TryGetValue(CultureHeaderCode, out var cultureList);

        if (cultureList == default(StringValues))
        {
            cultureList = new StringValues(DefaultCultureCode);
        }

        if (cultureList.Count == 1)
        {
            var cultureCode = cultureList.First()!;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
        }
        else if (cultureList.Count > 1)
        {
            throw new NotSupportedException("Too many CultureCode defined in client request.");
        }

        return await next(context);
    }
}
