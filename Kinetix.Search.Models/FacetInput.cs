namespace Kinetix.Search.Models;

/// <summary>
/// Entrée d'une facette en recherche.
/// </summary>
public class FacetInput
{
    public const string And = "and";
    public const string Or = "or";

    /// <summary>
    /// Opérateur utilisé entre les différentes valeurs ("and" ou "or").
    /// </summary>
    public string? Operator { get; set; }

    /// <summary>
    /// Valeurs de la facette à prendre.
    /// </summary>
    public IList<string> Selected { get; set; } = [];

    /// <summary>
    /// Valeurs de la facette à exclure.
    /// </summary>
    public IList<string> Excluded { get; set; } = [];
}
