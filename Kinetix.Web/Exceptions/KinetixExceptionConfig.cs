using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kinetix.Web.Exceptions;

public record KinetixExceptionConfig
{
    /// <summary>
    /// Format de retour des handlers d'exception par défaut de Kinetix.
    /// (le standard <see cref="ProblemDetails" /> ou le <see cref="KinetixErrorResponse" /> non-standard).
    /// </summary>
    public KinetixErrorFormat Format { get; set; } = KinetixErrorFormat.ProblemDetails;

    /// <summary>
    /// The operation that customizes the current <see cref="ProblemDetails" /> instance.
    /// </summary>
    public Action<ProblemDetailsContext>? CustomizeProblemDetails { get; set; }
}
