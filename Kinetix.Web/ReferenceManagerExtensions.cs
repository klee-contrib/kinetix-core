using Kinetix.Modeling.Exceptions;
using Kinetix.Services;
using Kinetix.Web.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Kinetix.Web;

/// <summary>
/// Extensions pour mapper les endpoints de ReferenceManager.
/// </summary>
public static class ReferenceManagerExtensions
{
    /// <summary>
    /// Mappe les endpoints pour l'accès (GET) et la purge (DELETE) des listes de références.
    /// </summary>
    /// <param name="endpoints">EndpointBuilder.</param>
    /// <param name="prefix">Préfixe des routes.</param>
    /// <returns>GroupBuilder.</returns>
    public static RouteGroupBuilder MapReferenceEndpoints(this IEndpointRouteBuilder endpoints, string prefix = "api/referenceList")
    {
        var group = endpoints.MapGroup(prefix);

        group.MapGet("{referenceName}", (string referenceName, [FromServices] IReferenceManager referenceManager) =>
        {
            if (!referenceManager.ReferenceLists.Contains(referenceName))
            {
                throw new BusinessException($"La liste de référence '{referenceName}' n'existe pas");
            }

            return referenceManager.GetReferenceList(referenceName);
        });

        group.MapDelete("{referenceName}", (string referenceName, [FromServices] IReferenceManager referenceManager) =>
        {
            if (!referenceManager.ReferenceLists.Contains(referenceName))
            {
                throw new BusinessException($"La liste de référence '{referenceName}' n'existe pas");
            }

            referenceManager.FlushCache(referenceName);
        });

        group.AddEndpointFilter<TransactionFilter>();

        return group;
    }
}
