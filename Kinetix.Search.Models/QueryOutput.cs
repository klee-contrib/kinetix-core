using System.Text.Json.Serialization;

namespace Kinetix.Search.Models;

/// <summary>
/// Sortie d'une recherche avancée.
/// </summary>
public class QueryOutput
{
    /// <summary>
    /// Groupe de liste de résultats.
    /// </summary>
    public required IList<GroupResult>? Groups { get; set; }

    /// <summary>
    /// Facettes sélectionnées.
    /// </summary>
    public required IList<FacetOutput> Facets { get; set; }

    /// <summary>
    /// Nombre total d'éléments.
    /// </summary>
    public required long TotalCount { get; set; }

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
