using System.Security;
using Microsoft.AspNetCore.Http;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler par défaut pour les SecurityException
/// </summary>
public class SecurityExceptionHandler : IKinetixExceptionHandler
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
}
