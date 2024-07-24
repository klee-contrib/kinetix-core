using System.Reflection;
using Kinetix.Services.DependencyInjection.Interceptors;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler d'exception pour Kinetix.
/// </summary>
internal class KinetixExceptionHandler(TelemetryClient? telemetryClient = null) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        while (exception is TargetInvocationException || exception is InterceptedException)
        {
            exception = exception switch
            {
                TargetInvocationException tex => tex.InnerException!,
                InterceptedException iex => iex.InnerException!,
                _ => exception
            };
        }

        telemetryClient?.TrackException(exception);

        IResult? result = null;

        foreach (var exceptionHandler in httpContext.RequestServices.GetRequiredService<IEnumerable<IKinetixExceptionHandler>>().OrderByDescending(eh => eh.Priority))
        {
            result = await exceptionHandler.Handle(exception);
            if (result != null)
            {
                break;
            }
        }

        result ??= DefaultExceptionHandler(exception);

        await result.ExecuteAsync(httpContext);

        return true;

    }

    private static IResult DefaultExceptionHandler(Exception ex)
    {
        var errors = new List<string> { ex.Message };

        while (ex.InnerException != null)
        {
            ex = ex.InnerException;
            errors.Add(ex.Message);
        }

        return Results.Json(new KinetixErrorResponse { Errors = errors }, statusCode: ex switch
        {
            BadHttpRequestException br => br.StatusCode,
            _ => 500
        });
    }
}
