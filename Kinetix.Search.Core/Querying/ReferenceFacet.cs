using System.Linq.Expressions;
using Kinetix.Modeling;
using Kinetix.Search.Models;
using Kinetix.Services;

namespace Kinetix.Search.Core.Querying;

/// <summary>
/// Facette de référence.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
public abstract class ReferenceFacet<TDocument> : TermFacet<TDocument>
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="code">Code de la facette.</param>
    /// <param name="label">Libellé de la facette.</param>
    /// <param name="field">Champ sur lequel agit la facette.</param>
    protected ReferenceFacet(string code, string label, Expression<Func<TDocument, object>> field)
        : base(code, label, field)
    {
    }

    /// <summary>
    /// Affiche l'intégralité des valeurs de la liste de référence dans les résultats de facettes, même si les buckets sont vides.
    /// </summary>
    public bool ShowEmptyReferenceValues { get; set; } = false;

    /// <summary>
    /// Récupère la liste de référence associée à la facette.
    /// </summary>
    /// <returns></returns>
    public abstract IList<FacetItem> GetReferenceList();
}

/// <summary>
/// Facette de référence.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
/// <typeparam name="T">Type de la référence.</typeparam>
public class ReferenceFacet<TDocument, T> : ReferenceFacet<TDocument>
{
    private readonly IReferenceManager _referenceManager;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="referenceManager">ReferenceManager.</param>
    /// <param name="code">Code de la facette.</param>
    /// <param name="label">Libellé de la facette.</param>
    /// <param name="field">Champ sur lequel agit la facette.</param>
    public ReferenceFacet(IReferenceManager referenceManager, string code, string label, Expression<Func<TDocument, object>> field)
        : base(code, label, field)
    {
        _referenceManager = referenceManager;
    }

    /// <inheritdoc />
    public override string ResolveLabel(string primaryKey)
    {
        return _referenceManager.GetReferenceValue<T>(primaryKey);
    }

    /// <inheritdoc />
    public override IList<FacetItem> GetReferenceList()
    {
        var def = BeanDescriptor.GetDefinition(typeof(T));
        return _referenceManager.GetReferenceList<T>()
            .Select(item => new FacetItem
            {
                Code = def.PrimaryKey.GetValue(item).ToString()!,
                Label = (string)def.DefaultProperty.GetValue(item),
                Count = 0
            })
            .ToList();
    }
}
