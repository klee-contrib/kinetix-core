using Kinetix.Modeling.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler par défaut pour les BusinessException.
/// </summary>
public class BusinessExceptionHandler : IKinetixExceptionHandler
{
    /// <inheritdoc />
    public int Priority => 1;

    /// <inheritdoc cref="IKinetixExceptionHandler.Handle" />
    public ValueTask<IResult?> Handle(Exception exception)
    {
        if (exception is not BusinessException be)
        {
            return ValueTask.FromResult<IResult?>(null);
        }

        var result = new KinetixErrorResponse { Code = be.Code };

        if (be.Errors != null && be.Errors.HasError)
        {
            foreach (var error in be.Errors)
            {
                result.Errors.Add(error.Message);
            }
        }

        if (!string.IsNullOrEmpty(be.BaseMessage))
        {
            result.Errors.Add(be.BaseMessage);
        }

        return ValueTask.FromResult<IResult?>(Results.BadRequest(result));
    }
}
