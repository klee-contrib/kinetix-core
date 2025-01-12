using System.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler par défaut pour les SecurityException
/// </summary>
public class SecurityExceptionHandler(KinetixExceptionConfig config, ProblemDetailsFactory problemDetailsFactory) : IKinetixExceptionHandler
{
    /// <inheritdoc />
    public int Priority => 1;

    /// <inheritdoc cref="IKinetixExceptionHandler.Handle" />
    public ValueTask<IResult?> Handle(Exception exception)
    {
        if (exception is not SecurityException)
        {
            return ValueTask.FromResult<IResult?>(null);
        }

        return ValueTask.FromResult<IResult?>(Results.Json(
            new KinetixErrorResponse { Errors = [exception.Message] },
            statusCode: 403));
    }


    /// <inheritdoc cref="IKinetixExceptionHandler.Handle" />
    public ValueTask<IResult?> Handle(Exception exception, HttpContext context)
    {
        IResult? result = null;

        if (exception is SecurityException)
        {
            if (config.Format == KinetixErrorFormat.Kinetix)
            {
                result = Results.Json(new KinetixErrorResponse { Errors = [exception.Message] }, statusCode: StatusCodes.Status403Forbidden);
            }
            else
            {
                result = Results.Problem(problemDetailsFactory.CreateProblemDetails(
                    context,
                    StatusCodes.Status403Forbidden,
                    detail: exception.Message));
            }
        }

        return ValueTask.FromResult(result);
    }
}
