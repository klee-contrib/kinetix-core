using Kinetix.Modeling.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler par défaut pour les BusinessException.
/// </summary>
public class BusinessExceptionHandler(KinetixExceptionConfig config, ProblemDetailsFactory problemDetailsFactory) : IKinetixExceptionHandler
{
    /// <inheritdoc />
    public int Priority => 1;

    /// <inheritdoc cref="IKinetixExceptionHandler.Handle" />
    public ValueTask<IResult?> Handle(Exception exception, HttpContext context)
    {
        IResult? result = null;

        if (exception is BusinessException be)
        {
            if (config.Format == KinetixErrorFormat.Kinetix)
            {
                var response = new KinetixErrorResponse { Code = be.Code };

                if (be.Errors != null && be.Errors.HasError)
                {
                    foreach (var error in be.Errors)
                    {
                        response.Errors.Add(error.Message);
                    }
                }

                if (!string.IsNullOrEmpty(be.BaseMessage))
                {
                    response.Errors.Add(be.BaseMessage);
                }

                result = Results.BadRequest(response);
            }
            else
            {
                var problemDetails = problemDetailsFactory.CreateProblemDetails(context, StatusCodes.Status400BadRequest);
                var errors = new List<string>();

                if (be.Errors != null && be.Errors.HasError)
                {
                    foreach (var error in be.Errors)
                    {
                        errors.Add(error.Message);
                    }
                }

                if (!string.IsNullOrEmpty(be.BaseMessage))
                {
                    errors.Add(be.BaseMessage);
                }

                problemDetails.Extensions.Add("errors", new Dictionary<string, List<string>>() { ["global"] = errors });

                if (be.Code != null)
                {
                    problemDetails.Extensions.Add("code", be.Code);
                }

                result = Results.Problem(problemDetails);
            }
        }

        return ValueTask.FromResult(result);
    }
}
