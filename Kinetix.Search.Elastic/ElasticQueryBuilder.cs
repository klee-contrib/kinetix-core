using Kinetix.Search.Core.DocumentModel;
using Nest;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Builder de requête ElasticSearch.
/// </summary>
public static class ElasticQueryBuilder
{
    /// <summary>
    /// Construit une requête pour une recherche textuelle.
    /// </summary>
    /// <param name="text">Texte de recherche.</param>
    /// <param name="fields">Champs de recherche.</param>
    /// <returns>Requête.</returns>
    public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildMultiMatchQuery<TDocument>(string text, params DocumentFieldDescriptor[] fields)
        where TDocument : class
    {
        return q => q.MultiMatch(m => m
            .Query(text)
            .Operator(Operator.And)
            .Type(TextQueryType.CrossFields)
            .Fields(fields.Select(f => new Field(f.FieldName, f.Boost)).ToArray()));
    }

    /// <summary>
    /// Construit une requête pour le filtrage exacte (sélection de facette, ...).
    /// </summary>
    /// <param name="field">Champ.</param>
    /// <param name="value">Valeur.</param>
    /// <param name="invert">Inverse le filtre.</param>
    /// <returns>Requête.</returns>
    public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildFilter<TDocument>(string field, string value, bool invert = false)
        where TDocument : class
    {
        QueryContainer query(QueryContainerDescriptor<TDocument> q) => q.Term(t => t.Field(field).Value(value));

        return invert
            ? (q => q.Bool(b => b.MustNot(query)))
            : query;
    }

    /// <summary>
    /// Construit une requête pour un champ qui doit manquer (équivalent d'une valeur NULL).
    /// </summary>
    /// <param name="field">Champ.</param>
    /// <param name="invert">Inverse le filtre.</param>
    /// <returns>Requête.</returns>
    public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildMissingField<TDocument>(string field, bool invert = false)
        where TDocument : class
    {
        QueryContainer query(QueryContainerDescriptor<TDocument> q) => q.Exists(t => t.Field(field));

        return invert
            ? query
            : (q => q.Bool(b => b.MustNot(query)));
    }

    /// <summary>
    /// Construit une requête avec des AND sur des sous-requêtes.
    /// </summary>
    /// <param name="subQueries">Sous-requêtes.</param>
    /// <returns>Requête.</returns>
    public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildAndQuery<TDocument>(params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] subQueries)
        where TDocument : class
    {
        return subQueries.Length switch
        {
            0 => q => q,
            1 => subQueries[0],
            _ => q => q.Bool(b => b.Filter(subQueries))
        };
    }

    /// <summary>
    /// Construit une requête avec des OR sur des sous-requêtes.
    /// </summary>
    /// <param name="subQueries">Sous-requêtes.</param>
    /// <returns>Requête.</returns>
    public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> BuildOrQuery<TDocument>(params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] subQueries)
    where TDocument : class
    {
        return subQueries.Length switch
        {
            0 => q => q,
            1 => subQueries[0],
            _ => q => q.Bool(b => b.Should(subQueries).MinimumShouldMatch(1))
        };
    }
}
