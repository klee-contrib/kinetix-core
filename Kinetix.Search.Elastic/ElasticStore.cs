using Kinetix.Monitoring.Core;
using Kinetix.Search.Core;
using Kinetix.Search.Core.Config;
using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Core.Querying;
using Kinetix.Search.Elastic.Querying;
using Kinetix.Search.Models;
using Microsoft.Extensions.Logging;
using Nest;

namespace Kinetix.Search.Elastic;

using static AdvancedQueryUtil;

/// <summary>
/// Store ElasticSearch.
/// </summary>
public class ElasticStore : ISearchStore
{
    private readonly AnalyticsManager _analytics;
    private readonly ElasticClient _client;
    private readonly SearchConfig _config;
    private readonly DocumentDescriptor _documentDescriptor;
    private readonly ElasticManager _elasticManager;
    private readonly FacetHandler _facetHandler;
    private readonly ElasticMappingFactory _factory;
    private readonly ILogger<ElasticStore> _logger;

    public ElasticStore(DocumentDescriptor documentDescriptor, ElasticClient client, ElasticManager elasticManager, ElasticMappingFactory factory, ILogger<ElasticStore> logger, FacetHandler facetHandler, AnalyticsManager analytics, SearchConfig config)
    {
        _analytics = analytics;
        _client = client;
        _config = config;
        _documentDescriptor = documentDescriptor;
        _elasticManager = elasticManager;
        _facetHandler = facetHandler;
        _factory = factory;
        _logger = logger;
    }

    /// <inheritdoc cref="ISearchStore.EnsureIndex" />
    public bool EnsureIndex<TDocument>()
        where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        var mapping = new PutMappingDescriptor<TDocument>()
            .Properties(selector => _factory.AddFields(selector, def.Fields));

        var indexCreated = _elasticManager.InitIndex<TDocument, DefaultIndexConfigurator>(mapping);

        if (indexCreated)
        {
            _logger.LogQuery(_analytics, "Map", () => _client.Map<TDocument>(_ => mapping));
        }

        return indexCreated;
    }


    /// <inheritdoc cref="ISearchStore.Get" />
    public TDocument Get<TDocument>(object key)
        where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        return _logger.LogQuery(_analytics, "Get", () => _client.Get(new DocumentPath<TDocument>(def.PrimaryKey.GetValueFromDocument(key)))).Source;
    }

    /// <inheritdoc cref="ISearchStore.Bulk" />
    public ISearchBulkDescriptor Bulk()
    {
        return new ElasticBulkDescriptor(_documentDescriptor, _client, _logger, _analytics);
    }

    /// <inheritdoc cref="ISearchStore.Delete" />
    public void Delete<TDocument>(object key, bool refresh = true)
        where TDocument : class
    {
        Bulk().Delete<TDocument>(key).Run(refresh);
    }

    /// <inheritdoc cref="ISearchStore.Index" />
    public void Index<TDocument>(TDocument document, bool refresh = true)
        where TDocument : class
    {
        if (document != null)
        {
            Bulk().Index(document).Run(refresh);
        }
    }

    /// <inheritdoc cref="ISearchStore.ResetIndex" />
    public int ResetIndex<TDocument>(IEnumerable<TDocument> documents, bool partialRebuild, ILogger? rebuildLogger = null)
        where TDocument : class
    {
        var indexName = SearchConfig.GetTypeNameForIndex(typeof(TDocument));
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));

        /* On vide l'index des documents obsolètes. */
        if (partialRebuild && def.IgnoreOnPartialRebuild?.OlderThanDays > 0 && def.PartialRebuildDate != null)
        {
            rebuildLogger?.LogInformation($"Partial rebuild. Deleting recent documents for {indexName}...");

            var deleteRes = _logger.LogQuery(_analytics, "DeleteAllByQuery", () => _client.DeleteByQuery<TDocument>(d =>
                d.Query(q => q.DateRange(d => d
                    .Field(def.PartialRebuildDate.FieldName)
                    .GreaterThan(DateTime.UtcNow.Date.AddDays(-def.IgnoreOnPartialRebuild.OlderThanDays))))
                .Timeout(TimeSpan.FromMinutes(5))
                .RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMinutes(5)))));

            rebuildLogger?.LogInformation($"{deleteRes.Deleted} documents deleted.");
        }

        rebuildLogger?.LogInformation($"Starting indexation for index {indexName}...");

        /* Indexation en cluster */
        var count = 0;

        try
        {
            _elasticManager.OptimizeIndexForReindex<TDocument>();

            foreach (var cluster in documents.Chunk(_config.ClusterSize))
            {
                Bulk().IndexMany(cluster).Run(false);
                count += cluster.Length;
                rebuildLogger?.LogInformation($"{count} documents indexed.");
            }

            rebuildLogger?.LogInformation($"Indexation of index {indexName} is complete.");
        }
        finally
        {
            _elasticManager.RevertOptimizeIndexForReindex<TDocument>();
        }

        return count;
    }

    /// <inheritdoc cref="ISearchStore.AdvancedQuery{TDocument, TOutput, TCriteria}(AdvancedQueryInput{TDocument, TCriteria}, Func{TDocument, TOutput})" />
    public QueryOutput<TOutput> AdvancedQuery<TDocument, TOutput, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper)
        where TDocument : class
        where TCriteria : ICriteria
    {
        return AdvancedQuery(input, (d, _) => documentMapper(d), filter: null, aggs: null);
    }

    /// <inheritdoc cref="ISearchStore.AdvancedQuery{TDocument, TOutput, TCriteria}(AdvancedQueryInput{TDocument, TCriteria}, Func{TDocument, IReadOnlyDictionary{string, IReadOnlyCollection{string}}, TOutput})" />
    public QueryOutput<TOutput> AdvancedQuery<TDocument, TOutput, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper)
        where TDocument : class
        where TCriteria : ICriteria
    {
        return AdvancedQuery(input, documentMapper, filter: null, aggs: null);
    }

    /// <inheritdoc cref="ISearchStore.MultiAdvancedQuery" />
    public IMultiAdvancedQueryDescriptor MultiAdvancedQuery()
    {
        return new MultiAdvancedQueryDescriptor(_client, _documentDescriptor, _facetHandler);
    }

    /// <inheritdoc cref="ISearchStore.AdvancedCount" />
    public long AdvancedCount<TDocument, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
        where TDocument : class
        where TCriteria : ICriteria
    {
        ArgumentNullException.ThrowIfNull(input);

        var def = _documentDescriptor.GetDefinition(typeof(TDocument));

        /* Requête de filtrage, qui inclus ici le filtre et le post-filtre puisqu'on ne fait pas d'aggrégations. */
        var filterQuery = GetFilterAndPostFilterQuery(def, input, _facetHandler);
        return _logger.LogQuery(_analytics, "AdvancedCount", () => _client
            .Count<TDocument>(s => s.Query(filterQuery)))
            .Count;
    }

    internal IEnumerable<TOutput> AdvancedQueryAll<TDocument, TOutput, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper, Func<QueryContainerDescriptor<TDocument>, QueryContainer>? filter)
       where TDocument : class
       where TCriteria : ICriteria
    {
        ArgumentNullException.ThrowIfNull(input);

        var def = _documentDescriptor.GetDefinition(typeof(TDocument));

        var pit = _logger.LogQuery(_analytics, "CreatePit", () => _client.OpenPointInTime(
               _config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(TDocument)),
               p => p.KeepAlive("1m")));

        var pitId = pit.Id;
        try
        {
            object[]? searchAfter = null;

            var search = true;
            do
            {
                var res = _logger.LogQuery(_analytics, $"AdvancedQueryWithPit", () => _client.Search(
                    GetAdvancedQueryDescriptor(def, input, _facetHandler, filter, pitId: pitId, searchAfter: searchAfter)));

                foreach (var doc in res.Hits)
                {
                    yield return documentMapper(doc.Source, doc.Highlight);
                }

                if (res.Documents.Count == 10000)
                {
                    searchAfter = res.Hits.Last().Sorts.ToArray();
                }
                else
                {
                    search = false;
                }
            } while (search);
        }
        finally
        {
            _logger.LogQuery(_analytics, "DeletePit", () => _client.ClosePointInTime(p => p.Id(pitId)));
        }
    }

    internal QueryOutput<TOutput> AdvancedQuery<TDocument, TOutput, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper, Func<QueryContainerDescriptor<TDocument>, QueryContainer>? filter, Action<AggregationContainerDescriptor<TDocument>>? aggs)
       where TDocument : class
       where TCriteria : ICriteria
    {
        ArgumentNullException.ThrowIfNull(input);

        /* Définition du document. */
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));

        /* Facettage. */
        var facetDefList = input.FacetQueryDefinition.Facets;
        var hasFacet = facetDefList.Count != 0;
        /* Group */
        var groupFieldName = GetGroupFieldName(input);
        var hasGroup = groupFieldName != null;

        var res = _logger.LogQuery(_analytics, "AdvancedQuery", () => _client.Search(
            GetAdvancedQueryDescriptor(def, input, _facetHandler, filter, aggs, facetDefList, groupFieldName)));

        /* Extraction des facettes. */
        var facetListOutput = new List<FacetOutput>();
        if (hasFacet)
        {
            foreach (var facetDef in facetDefList)
            {
                facetListOutput.Add(new FacetOutput
                {
                    Code = facetDef.Code,
                    Label = facetDef.Label,
                    IsMultiSelectable = facetDef.IsMultiSelectable,
                    IsMultiValued = def.Fields[facetDef.FieldName].IsMultiValued,
                    CanExclude = facetDef.CanExclude,
                    Values = _facetHandler.ExtractFacetItemList(res.Aggregations, facetDef)
                });
            }
        }

        /* Ajout des valeurs de facettes manquantes (cas d'une valeur demandée par le client non trouvée par la recherche.) */
        foreach (var facet in input.SearchCriteria.SelectMany(sc => sc.Facets ?? []))
        {
            var facetItems = facetListOutput.Single(f => f.Code == facet.Key).Values;
            /* On ajoute un FacetItem par valeur non trouvée, avec un compte de 0. */
            foreach (var value in facet.Value.Selected.Concat(facet.Value.Excluded))
            {
                if (!facetItems.Any(f => f.Code == value))
                {
                    var label = value == FacetConst.NotNullValue
                        ? FacetConst.NotNullLabel
                        : value == FacetConst.NullValue
                            ? FacetConst.NullLabel
                            : facetDefList.FirstOrDefault(fct => fct.Code == facet.Key)?.ResolveLabel(value);

                    facetItems.Add(new FacetItem
                    {
                        Code = value,
                        Label = label!,
                        Count = 0
                    });
                }
            }
        }

        /* Extraction des résultats. */
        var resultList = new List<TOutput>();
        var groupResultList = new List<GroupResult<TOutput>>();
        if (hasGroup)
        {
            /* Groupement. */
            var bucket = res.Aggregations.Terms(groupFieldName);
            bucket ??= res.Aggregations.Filter(groupFieldName).Terms(groupFieldName);

            foreach (var group in bucket.Buckets)
            {
                var list = group.TopHits(TopHitName).Hits<TDocument>().Select(d => documentMapper(d.Source, d.Highlight)).ToList();
                groupResultList.Add(new GroupResult<TOutput>
                {
                    Code = group.Key.ToString(),
                    Label = facetDefList.First(f => f.Code == input.SearchCriteria.First(sc => !string.IsNullOrEmpty(sc.Group)).Group).ResolveLabel(group.Key),
                    List = list,
                    TotalCount = (int)(group.DocCount ?? 0)
                });
            }

            /* Groupe pour les valeurs missing. */
            var missingBucket = res.Aggregations.Missing(groupFieldName + MissingGroupPrefix);
            missingBucket ??= res.Aggregations.Filter(groupFieldName).Missing(groupFieldName + MissingGroupPrefix);

            var nullDocs = missingBucket.TopHits(TopHitName).Hits<TDocument>().Select(d => documentMapper(d.Source, d.Highlight)).ToList();
            if (nullDocs.Count != 0)
            {
                groupResultList.Add(new GroupResult<TOutput>
                {
                    Code = FacetConst.NullValue,
                    Label = input.FacetQueryDefinition.FacetNullValueLabel ?? "focus.search.results.missing",
                    List = nullDocs,
                    TotalCount = (int)missingBucket.DocCount
                });
            }

            resultList = null;
        }
        else
        {
            /* Liste unique. */
            resultList = res.Hits.Select(h => documentMapper(h.Source, h.Highlight)).ToList();
            groupResultList = null;
        }

        /* Construction de la sortie. */
        return new QueryOutput<TOutput>
        {
            List = resultList,
            Facets = facetListOutput,
            Groups = groupResultList,
            SearchFields = def.SearchFields.Select(tf => tf.FieldName).ToList(),
            TotalCount = (int)res.Total,
            Aggregations = res.Aggregations
        };
    }
}
