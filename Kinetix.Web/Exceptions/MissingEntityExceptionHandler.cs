using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler par défaut pour les "Single" en erreur dans EF Core et "KeyNotFoundException".
/// </summary>
public class MissingEntityExceptionHandler(KinetixExceptionConfig config, ProblemDetailsFactory problemDetailsFactory) : IKinetixExceptionHandler
{
    /// <inheritdoc />
    public int Priority => 1;

    /// <inheritdoc cref="IKinetixExceptionHandler.Handle" />
    public ValueTask<IResult?> Handle(Exception exception, HttpContext context)
    {
        IResult? result = null;

        if (exception is InvalidOperationException { Source: "Microsoft.EntityFrameworkCore" } or KeyNotFoundException)
        {
            var message = exception is KeyNotFoundException ke && ke.Message != new KeyNotFoundException().Message ? ke.Message : "L'objet demandé n'existe pas.";

            if (config.Format == KinetixErrorFormat.Kinetix)
            {
                result = Results.NotFound(new KinetixErrorResponse { Errors = [message] });
            }
            else
            {
                result = Results.Problem(problemDetailsFactory.CreateProblemDetails(
                    context,
                    StatusCodes.Status404NotFound,
                    detail: message));
            }
        }

        return ValueTask.FromResult(result);
    }
}
