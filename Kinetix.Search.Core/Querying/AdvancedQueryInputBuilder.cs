using Kinetix.Search.Models;
using Kinetix.Services;

namespace Kinetix.Search.Core.Querying;

/// <summary>
/// Builder de requête pour la recherche avancée.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
public class AdvancedQueryInputBuilder<TDocument> : AdvancedQueryInputBuilder<TDocument, DefaultCriteria>
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="referenceManager">ReferenceManager.</param>
    /// <param name="searchCriteria">Requêtes.</param>
    public AdvancedQueryInputBuilder(IReferenceManager referenceManager, params QueryInput<DefaultCriteria>[] searchCriteria)
        : base(referenceManager, searchCriteria)
    {
    }
}

/// <summary>
/// Builder de requête pour la recherche avancée.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
/// <typeparam name="TCriteria">Type du critère.</typeparam>
public class AdvancedQueryInputBuilder<TDocument, TCriteria>
 where TCriteria : ICriteria
{
    private readonly IReferenceManager _referenceManager;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="referenceManager">ReferenceManager.</param>
    /// <param name="searchCriteria">Requêtes.</param>
    public AdvancedQueryInputBuilder(IReferenceManager referenceManager, params QueryInput<TCriteria>[] searchCriteria)
    {
        _referenceManager = referenceManager;
        Input.SearchCriteria = searchCriteria;
    }

    /// <summary>
    /// Entrée de recherche avancée.
    /// </summary>
    public AdvancedQueryInput<TDocument, TCriteria> Input { get; } = new AdvancedQueryInput<TDocument, TCriteria>();

    /// <summary>
    /// Renseigne des critères additionnels (en plus de ceux de l'ApiInput).
    /// </summary>
    /// <param name="criteria">Critères.</param>
    /// <returns>AdvancedQueryInputBuilder.</returns>
    public AdvancedQueryInputBuilder<TDocument, TCriteria> WithAdditionalCriteria(TDocument criteria)
    {
        Input.AdditionalCriteria = criteria;
        return this;
    }

    /// <summary>
    /// Renseigne les facettes.
    /// </summary>
    /// <param name="facetsDefinition">Définitions de facettes.</param>
    /// <returns>AdvancedQueryInputBuilder.</returns>
    public AdvancedQueryInputBuilder<TDocument, TCriteria> WithFacets(Action<FacetQueryDefinitionBuilder<TDocument>> facetsDefinition)
    {
        var builder = new FacetQueryDefinitionBuilder<TDocument>(_referenceManager);
        facetsDefinition(builder);
        Input.FacetQueryDefinition = builder.Definition;
        return this;
    }

    /// <summary>
    /// Renseigne le nombre de résultats max par groupe.
    /// </summary>
    /// <param name="groupSize">Nombre souhaité.</param>
    /// <returns>AdvancedQueryInputBuilder.</returns>
    public AdvancedQueryInputBuilder<TDocument, TCriteria> WithGroupSize(int groupSize)
    {
        Input.GroupSize = groupSize;
        return this;
    }

    /// <summary>
    /// Renseigne le filtre de sécurité.
    /// </summary>
    /// <param name="security">Filtre de sécurité.</param>
    /// <returns>AdvancedQueryInputBuilder.</returns>
    public AdvancedQueryInputBuilder<TDocument, TCriteria> WithSecurity(params string[] security)
    {
        Input.Security = security?.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        return this;
    }

    /// <summary>
    /// Ajoute les highlights sur la recherche texte en retour.
    /// </summary>
    /// <returns>AdvancedQueryInputBuilder.</returns>
    public AdvancedQueryInputBuilder<TDocument, TCriteria> WithHighlights()
    {
        Input.Highlights = true;
        return this;
    }
}
