using Kinetix.Search.Models;

namespace Kinetix.Search.Core.Querying;

/// <summary>
/// Entrée complète d'une recherche avancée.
/// </summary>
public class AdvancedQueryInput<TDocument, TCriteria>
     where TCriteria : ICriteria
{
    /// <summary>
    /// Critères de recherche, combinés en "ou".
    /// </summary>
    public IEnumerable<QueryInput<TCriteria>> SearchCriteria
    {
        get;
        set;
    } = [];

    /// <summary>
    /// Définition de la recherhe à facette.
    /// </summary>
    public FacetQueryDefinition<TDocument> FacetQueryDefinition
    {
        get;
        set;
    } = new FacetQueryDefinition<TDocument>();

    /// <summary>
    /// Filtrage de sécurité.
    /// </summary>
    public string[]? Security
    {
        get;
        set;
    }

    /// <summary>
    /// Critères supplémentaires.
    /// </summary>
    public TDocument? AdditionalCriteria
    {
        get;
        set;
    }

    /// <summary>
    /// Nombre d'éléments à récupérer dans un groupe.
    /// </summary>
    public int GroupSize { get; set; } = 10;

    /// <summary>
    /// Retourne les highlights sur la recherche texte.
    /// </summary>
    public bool Highlights { get; set; }
}
