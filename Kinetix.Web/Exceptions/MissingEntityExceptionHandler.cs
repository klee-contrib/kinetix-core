using Microsoft.AspNetCore.Http;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler par défaut pour les "Single" en erreur dans EF Core.
/// </summary>
public class MissingEntityExceptionHandler : IKinetixExceptionHandler
{
    /// <inheritdoc />
    public int Priority => 1;

    /// <inheritdoc cref="IKinetixExceptionHandler.Handle" />
    public ValueTask<IResult?> Handle(Exception exception)
    {
        if (exception is not InvalidOperationException { Source: "Microsoft.EntityFrameworkCore" })
        {
            return ValueTask.FromResult<IResult?>(null);
        }

        return ValueTask.FromResult<IResult?>(Results.NotFound(new KinetixErrorResponse { Errors = ["L'objet demandé n'existe pas."] }));
    }
}
