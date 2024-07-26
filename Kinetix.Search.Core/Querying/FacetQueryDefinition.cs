namespace Kinetix.Search.Core.Querying;

/// <summary>
/// Définition d'une recherche à facettes.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
public class FacetQueryDefinition<TDocument>
{
    /// <summary>
    /// Créé une nouvelle instance de FacetQueryDefinition.
    /// </summary>
    /// <param name="facets">Facettes.</param>
    public FacetQueryDefinition(params IFacetDefinition<TDocument>[] facets)
    {
        Facets = facets.ToList();
    }

    /// <summary>
    /// Libellé de la valeur de facette nulle.
    /// </summary>
    public string? FacetNullValueLabel
    {
        get;
        set;
    }

    /// <summary>
    /// Liste des facettes.
    /// </summary>
    public ICollection<IFacetDefinition<TDocument>> Facets
    {
        get;
        private set;
    }
}
