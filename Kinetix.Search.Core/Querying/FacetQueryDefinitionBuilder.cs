using System.Linq.Expressions;
using Kinetix.Services;

namespace Kinetix.Search.Core.Querying;

/// <summary>
/// Builder de définitions de facettes.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
public class FacetQueryDefinitionBuilder<TDocument>
{
    private readonly IReferenceManager _referenceManager;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="referenceManager">ReferenceManager.</param>
    public FacetQueryDefinitionBuilder(IReferenceManager referenceManager)
    {
        _referenceManager = referenceManager;
    }

    /// <summary>
    /// Définition des facettes.
    /// </summary>
    public FacetQueryDefinition<TDocument> Definition { get; } = new FacetQueryDefinition<TDocument>();

    /// <summary>
    /// Enregistre une facette.
    /// </summary>
    /// <param name="facet">Facette.</param>
    /// <returns>FacetQueryDefinitionBuilder.</returns>
    public FacetQueryDefinitionBuilder<TDocument> Add(IFacetDefinition<TDocument> facet)
    {
        Definition.Facets.Add(facet);
        return this;
    }

    /// <summary>
    /// Enregistre une facette sur un booléen.
    /// </summary>
    /// <param name="code">Code de la facette.</param>
    /// <param name="label">Libellé de la facette.</param>
    /// <param name="field">Champ sur lequel agit la facette.</param>
    /// <param name="configurator">Configurateur de facette.</param>
    /// <returns>FacetQueryDefinitionBuilder.</returns>
    public FacetQueryDefinitionBuilder<TDocument> AddBoolean(string code, string label, Expression<Func<TDocument, object>> field, Action<BooleanFacet<TDocument>>? configurator = null)
    {
        var facet = new BooleanFacet<TDocument>(code, label, field);
        configurator?.Invoke(facet);
        Add(facet);
        return this;
    }

    /// <summary>
    /// Enregistre une facette de date.
    /// </summary>
    /// <param name="code">Code de la facette.</param>
    /// <param name="label">Libellé de la facette.</param>
    /// <param name="field">Champ sur lequel agit la facette.</param>
    /// <param name="configurator">Configurateur de facette.</param>
    /// <returns>FacetQueryDefinitionBuilder.</returns>
    public FacetQueryDefinitionBuilder<TDocument> AddDate(string code, string label, Expression<Func<TDocument, object>> field, Action<DateFacet<TDocument>>? configurator = null)
    {
        var facet = new DateFacet<TDocument>(code, label, field);
        configurator?.Invoke(facet);
        Add(facet);
        return this;
    }

    /// <summary>
    /// Enregistre une facette sur l'existence d'un champ.
    /// </summary>
    /// <param name="code">Code de la facette.</param>
    /// <param name="label">Libellé de la facette.</param>
    /// <param name="field">Champ sur lequel agit la facette.</param>
    /// <returns>FacetQueryDefinitionBuilder.</returns>
    public FacetQueryDefinitionBuilder<TDocument> AddExists(string code, string label, Expression<Func<TDocument, object>> field)
    {
        var facet = new ExistsFacet<TDocument>(code, label, field);
        Add(facet);
        return this;
    }

    /// <summary>
    /// Enregistre une facette de référence.
    /// </summary>
    /// <typeparam name="T">Type de la liste de référence.</typeparam>
    /// <param name="code">Code de la facette.</param>
    /// <param name="label">Libellé de la facette.</param>
    /// <param name="field">Champ sur lequel agit la facette.</param>
    /// <param name="configurator">Configurateur de facette.</param>
    /// <returns>FacetQueryDefinitionBuilder.</returns>
    public FacetQueryDefinitionBuilder<TDocument> AddReference<T>(string code, string label, Expression<Func<TDocument, object>> field, Action<ReferenceFacet<TDocument>>? configurator = null)
        where T : class
    {
        var facet = new ReferenceFacet<TDocument, T>(_referenceManager, code, label, field);
        configurator?.Invoke(facet);
        Add(facet);
        return this;
    }

    /// <summary>
    /// Enregistre une facette simple.
    /// </summary>
    /// <param name="code">Code de la facette.</param>
    /// <param name="label">Libellé de la facette.</param>
    /// <param name="field">Champ sur lequel agit la facette.</param>
    /// <param name="configurator">Configurateur de facette.</param>
    /// <returns>FacetQueryDefinitionBuilder.</returns>
    public FacetQueryDefinitionBuilder<TDocument> AddTerm(string code, string label, Expression<Func<TDocument, object>> field, Action<TermFacet<TDocument>>? configurator = null)
    {
        var facet = new TermFacet<TDocument>(code, label, field);
        configurator?.Invoke(facet);
        Add(facet);
        return this;
    }

    /// <summary>
    /// Modifie le libellé de la valeur de facette correspond au bucket "missing".
    /// </summary>
    /// <param name="nullLabel">Libellé.</param>
    /// <returns>FacetQueryDefinitionBuilder.</returns>
    public FacetQueryDefinitionBuilder<TDocument> WithNullLabel(string nullLabel)
    {
        Definition.FacetNullValueLabel = nullLabel;
        return this;
    }
}
