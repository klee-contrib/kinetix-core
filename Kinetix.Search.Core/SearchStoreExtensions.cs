using System.Diagnostics;
using Kinetix.Search.Core.Querying;
using Kinetix.Search.Models;

namespace Kinetix.Search.Core;

/// <summary>
/// Extensions pour ISearchStore.
/// </summary>
public static class SearchStoreExtensions
{
    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="queryInput">Query input.</param>
    /// <returns>Résultat.</returns>
    public static (IEnumerable<TDocument> data, int totalCount) Query<TDocument>(this ISearchStore store, BasicQueryInput<TDocument> queryInput)
        where TDocument : class
    {
        return store.Query(queryInput, x => x);
    }

    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="queryInput">Query input.</param>
    /// <param name="documentMapper">Mapper de document.</param>
    /// <returns>Résultat.</returns>
    public static (IEnumerable<TOutput> data, int totalCount) Query<TDocument, TOutput>(this ISearchStore store, BasicQueryInput<TDocument> queryInput, Func<TDocument, TOutput> documentMapper)
        where TDocument : class
    {
        return store.Query(queryInput, (d, _) => documentMapper(d));
    }

    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="queryInput">Query input.</param>
    /// <param name="documentMapper">Mapper de document.</param>
    /// <returns>Résultat.</returns>
    public static (IEnumerable<TOutput> data, int totalCount) Query<TDocument, TOutput>(this ISearchStore store, BasicQueryInput<TDocument> queryInput, Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper)
        where TDocument : class
    {
        return store.Query(queryInput, new DefaultCriteria { Query = queryInput.Query }, documentMapper);
    }

    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="criteria">Critère de recherche.</param>
    /// <param name="documentMapper">Mapper de document.</param>
    /// <returns>Résultat.</returns>
    public static (IEnumerable<TOutput> data, int totalCount) Query<TDocument, TCriteria, TOutput>(this ISearchStore store, TCriteria criteria, Func<TDocument, TOutput> documentMapper)
        where TDocument : class
        where TCriteria : ICriteria
    {
        return store.Query(null, criteria, documentMapper);
    }

    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="criteria">Critère de recherche.</param>
    /// <param name="documentMapper">Mapper de document.</param>
    /// <returns>Résultat.</returns>
    public static (IEnumerable<TOutput> data, int totalCount) Query<TDocument, TCriteria, TOutput>(this ISearchStore store, TCriteria criteria, Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper)
        where TDocument : class
        where TCriteria : ICriteria
    {
        return store.Query(null, criteria, documentMapper);
    }

    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="queryInput">Query input.</param>
    /// <param name="criteria">Critère de recherche.</param>
    /// <param name="documentMapper">Mapper de document.</param>
    /// <returns>Résultat.</returns>
    public static (IEnumerable<TOutput> data, int totalCount) Query<TDocument, TCriteria, TOutput>(this ISearchStore store, BasicQueryInput<TDocument>? queryInput, TCriteria criteria, Func<TDocument, TOutput> documentMapper)
        where TDocument : class
        where TCriteria : ICriteria
    {
        return store.Query(queryInput, criteria, (d, _) => documentMapper(d));
    }

    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="queryInput">Query input.</param>
    /// <param name="criteria">Critère de recherche.</param>
    /// <param name="documentMapper">Mapper de document.</param>
    /// <returns>Résultat.</returns>
    public static (IEnumerable<TOutput> data, int totalCount) Query<TDocument, TCriteria, TOutput>(this ISearchStore store, BasicQueryInput<TDocument>? queryInput, TCriteria criteria, Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper)
        where TDocument : class
        where TCriteria : ICriteria
    {
        if (string.IsNullOrEmpty(criteria.Query))
        {
            return (new List<TOutput>(), 0);
        }

        var input = new AdvancedQueryInput<TDocument, TCriteria>
        {
            SearchCriteria =
            [
                new QueryInput<TCriteria>
                {
                    Criteria = criteria,
                    Skip = 0,
                    Top = queryInput?.Top ?? 10
                }
            ],
            Security = queryInput?.Security,
            AdditionalCriteria = queryInput?.Criteria
        };

        var output = store.AdvancedQuery(input, documentMapper);
        return (output.List!, output.TotalCount);
    }
}
