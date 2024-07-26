namespace Kinetix.Search.Core.Querying;

/// <summary>
/// Entrée complète d'une recherche simple.
/// </summary>
public class BasicQueryInput<TDocument>
{
    /// <summary>
    /// Requête full text.
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Nombre de résultats à retourner.
    /// </summary>
    public int? Top { get; set; }

    /// <summary>
    /// Filtrage de sécurité.
    /// </summary>
    public string[]? Security { get; set; }

    /// <summary>
    /// Filtres.
    /// </summary>
    public TDocument? Criteria { get; set; }
}
