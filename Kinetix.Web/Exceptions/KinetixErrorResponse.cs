namespace Kinetix.Web.Exceptions;

/// <summary>
/// Réponse en erreur.
/// </summary>
public class KinetixErrorResponse
{
    /// <summary>
    /// Code éventuel de l'erreur.
    /// </summary>
    public string? Code { get; set; }

    public IList<string> Errors { get; set; } = [];
}
