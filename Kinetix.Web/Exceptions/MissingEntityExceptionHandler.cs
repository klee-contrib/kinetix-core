using Microsoft.AspNetCore.Http;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler par défaut pour les "Single" en erreur dans EF Core et "KeyNotFoundException".
/// </summary>
public class MissingEntityExceptionHandler : IKinetixExceptionHandler
{
    /// <inheritdoc />
    public int Priority => 1;

    /// <inheritdoc cref="IKinetixExceptionHandler.Handle" />
    public ValueTask<IResult?> Handle(Exception exception)
    {
        if (exception is not InvalidOperationException { Source: "Microsoft.EntityFrameworkCore" } and not KeyNotFoundException)
        {
            return ValueTask.FromResult<IResult?>(null);
        }

        var message = exception is KeyNotFoundException ke && ke.Message != new KeyNotFoundException().Message ? ke.Message : "L'objet demandé n'existe pas.";

        return ValueTask.FromResult<IResult?>(Results.NotFound(new KinetixErrorResponse { Errors = [message] }));
    }
}
