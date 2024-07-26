using System.Text.Json.Serialization;

namespace Kinetix.Search.Models;

/// <summary>
/// Sortie d'une recherche avancée.
/// </summary>
/// <typeparam name="TDocument">Le type du document.</typeparam>
public class QueryOutput<TDocument>
{
    /// <summary>
    /// Liste de résultats (cas d'une recherche sans groupe).
    /// </summary>
    public IList<TDocument>? List { get; set; }

    /// <summary>
    /// Groupe de liste de résultats (cas d'une recherche avec groupe).
    /// </summary>
    public IList<GroupResult<TDocument>>? Groups { get; set; }

    /// <summary>
    /// Facettes sélectionnées.
    /// </summary>
    public IList<FacetOutput> Facets { get; set; } = [];

    /// <summary>
    /// Champs de recherche disponibles.
    /// </summary>
    public IList<string> SearchFields { get; set; } = [];

    /// <summary>
    /// Nombre total d'éléments.
    /// </summary>
    public required int TotalCount { get; set; }

    /// <summary>
    /// Token retourné pour la pagination.
    /// </summary>
    public string? SkipToken { get; set; }

    /// <summary>
    /// Conteneur d'agrégations.
    /// </summary>
    [JsonIgnore]
    public object? Aggregations { get; set; }
}
