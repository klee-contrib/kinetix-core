using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Core.Querying;
using Kinetix.Search.Models;
using Nest;

namespace Kinetix.Search.Elastic;

using static ElasticQueryBuilder;

/// <summary>
/// Handler de facette.
/// </summary>
public class FacetHandler
{
    private const string MissingFacetPrefix = "_Missing";
    private readonly DocumentDescriptor _documentDescriptor;

    /// <summary>
    /// Créé une nouvelle instance de StandardFacetHandler.
    /// </summary>
    /// <param name="documentDescriptor">Descripteur de documents.</param>
    public FacetHandler(DocumentDescriptor documentDescriptor)
    {
        _documentDescriptor = documentDescriptor;
    }

    /// <summary>
    /// Construit le filtre pour une facette multi sélectionnable.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    /// <param name="input">Entrée de facette.</param>
    /// <param name="facetDef">Définition de facette.</param>
    /// <param name="isMultiValued">Si multi valué.</param>
    /// <returns></returns>
    public Func<QueryContainerDescriptor<TDocument>, QueryContainer>? BuildMultiSelectableFilter<TDocument>(FacetInput input, IFacetDefinition<TDocument> facetDef, bool isMultiValued)
         where TDocument : class
    {
        if (!isMultiValued && input.Selected.Any() && input.Excluded.Any())
        {
            throw new ElasticException($@"Single valued facet ""{facetDef.Code}"" cannot have both selected and excluded fields");
        }

        var queries = input.Selected.Select(sf => CreateFacetSubQuery(sf, false, facetDef))
            .Concat(input.Excluded.Select(sf => CreateFacetSubQuery(sf, true, facetDef)))
            .ToArray();

        return !queries.Any()
                ? null
                : isMultiValued && input.Operator == FacetInput.And || !isMultiValued && input.Excluded.Any()
                    ? BuildAndQuery(queries)
                    : BuildOrQuery(queries);
    }

    /// <summary>
    /// Créé la sous-requête de filtrage pour la facette sélectionnée.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    /// <param name="facet">Sélection de facette.</param>
    /// <param name="exclude">Exclut les valeurs pour lesquelles la facette correspond au lieu de les inclure.</param>
    /// <param name="facetDef">Définition de la facette.</param>
    public Func<QueryContainerDescriptor<TDocument>, QueryContainer> CreateFacetSubQuery<TDocument>(string facet, bool exclude, IFacetDefinition<TDocument> facetDef)
        where TDocument : class
    {
        /* Traite la valeur de sélection NULL */
        return facet switch
        {
            FacetConst.NullValue => BuildMissingField<TDocument>(facetDef.FieldName, exclude),
            FacetConst.NotNullValue => BuildMissingField<TDocument>(facetDef.FieldName, !exclude),
            _ => BuildFilter<TDocument>(facetDef.FieldName, facet, exclude)
        };
    }

    /// <summary>
    /// Définit l'agrégation correspondant à la facette lors de la recherche à facettes.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    /// <param name="agg">Descripteur d'agrégation.</param>
    /// <param name="facet">Définition de la facet.</param>
    /// <param name="facetList">Définitions de toutes les facettes.</param>
    /// <param name="inputFacetsList">Facettes sélectionnées, pour filtrer (si plusieurs, combinées en "ou").</param>
    public void DefineAggregation<TDocument>(AggregationContainerDescriptor<TDocument> agg, IFacetDefinition<TDocument> facet, ICollection<IFacetDefinition<TDocument>> facetList, IEnumerable<IDictionary<string, FacetInput>> inputFacetsList)
        where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));

        AggregationContainerDescriptor<TDocument> AggDescriptor(AggregationContainerDescriptor<TDocument> aa)
        {
            /* Crée une agrégation sur les valeurs discrètes du champ. */
            if (facet is ExistsFacet<TDocument>)
            {
                aa.Filter(facet.Code, f => f
                    .Filter(ff => ff
                        .Exists(e => e.Field(facet.Field))));
            }
            else
            {
                aa.Terms(facet.Code, st => st
                    .Field(facet.Field)
                    .Size(200)
                    .Order(t => facet.Ordering switch
                    {
                        FacetOrdering.KeyAscending => t.KeyAscending(),
                        FacetOrdering.KeyDescending => t.KeyDescending(),
                        FacetOrdering.CountAscending => t.CountAscending(),
                        _ => t.CountDescending(),
                    }));
            }

            /* Crée une agrégation pour les valeurs non renseignées du champ. */
            if (facet.HasMissing)
            {
                aa.Filter(facet.Code + MissingFacetPrefix, f => f
                    .Filter(ff => ff
                        .Bool(b => b.MustNot(ee => ee.Exists(e => e.Field(facet.Field))))));
            }

            return aa;
        };

        /* On construit la requête de filtrage sur les autres facettes multi-sélectionnables. */
        var filtersList = inputFacetsList
            .Select(inputFacets => inputFacets
                .Select(inf =>
                {
                    /* On ne filtre pas sur la facette en cours. */
                    if (inf.Key == facet.Code)
                    {
                        return null!;
                    }

                    var targetFacet = facetList.Single(f => f.Code == inf.Key);

                    /* On ne filtre pas sur les facettes non multisélectionnables. */
                    return !targetFacet.IsMultiSelectable
                        ? null!
                        : BuildMultiSelectableFilter(inf.Value, targetFacet, def.Fields[targetFacet.FieldName].IsMultiValued)!;
                })
                .Where(sf => sf != null)
                .ToArray())
            .Where(inputFacets => inputFacets.Any());

        if (!filtersList.Any() || inputFacetsList.Count() > 1)
        {
            AggDescriptor(agg);
        }
        else
        {
            agg.Filter(facet.Code, f => f
                /* Crée le filtre sur les facettes multi-sélectionnables. */
                .Filter(BuildOrQuery(filtersList.Select(BuildAndQuery).ToArray()))
                .Aggregations(AggDescriptor));
        }
    }

    /// <summary>
    /// Extrait les facets du résultat d'une requête.
    /// </summary>
    /// <typeparam name="TDocument">Type de document.</typeparam>
    /// <param name="aggs">Aggrégations Elastic.</param>
    /// <param name="facetDef">Définition de la facette.</param>
    /// <returns>Sortie des facettes.</returns>
    public ICollection<FacetItem> ExtractFacetItemList<TDocument>(AggregateDictionary aggs, IFacetDefinition<TDocument> facetDef)
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        var propType = def.Fields[facetDef.FieldName].PropertyType;
        var isDate = propType == typeof(DateTime) || propType == typeof(DateTime?);

        var facetOutput = new List<FacetItem>();

        /* Valeurs renseignées. */
        if (facetDef is ExistsFacet<TDocument>)
        {
            var bucket = aggs.Filter(facetDef.Code);

            // Si on a un filtre sur la facette, alors le premier bucket qu'on a récupère c'est celui là. 
            // Celui qui nous intéresse (avec les vrais résultats) c'est donc le sous-bucket.
            // On distingue les cas en regardant s'il y a un sous-bucket ou non.
            var subBucket = bucket.Filter(facetDef.Code);
            if (subBucket != null)
            {
                bucket = subBucket;
            }

            if (bucket.DocCount > 0)
            {
                facetOutput.Add(new FacetItem { Code = FacetConst.NotNullValue, Label = FacetConst.NotNullLabel, Count = bucket.DocCount });
            }
        }
        else
        {
            var bucket = aggs.Terms(facetDef.Code);
            bucket ??= aggs.Filter(facetDef.Code).Terms(facetDef.Code);

            foreach (var b in bucket.Buckets)
            {
                // Pour une raison inconnue, ES renvoie un timestamp au lieu de la date dans son format original...
                var code = isDate
                    ? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                        .AddMilliseconds(long.Parse(b.Key))
                        .ToString("yyyy-MM-ddTHH:mm:ssZ")
                    : b.Key;

                facetOutput.Add(new FacetItem { Code = code, Label = facetDef.ResolveLabel(code), Count = b.DocCount ?? 0 });
            }
        }

        /* Valeurs non renseignées. */
        if (facetDef.HasMissing)
        {
            var bucket = aggs.Filter(facetDef.Code + MissingFacetPrefix);
            bucket ??= aggs.Filter(facetDef.Code).Filter(facetDef.Code + MissingFacetPrefix);

            if (bucket.DocCount > 0)
            {
                facetOutput.Add(new FacetItem { Code = FacetConst.NullValue, Label = FacetConst.NullLabel, Count = bucket.DocCount });
            }
        }

        // Gestion des modes spéciaux sur les facettes de référence.
        if (facetDef is ReferenceFacet<TDocument> rfDef && (rfDef.ShowEmptyReferenceValues || rfDef.Ordering == FacetOrdering.ReferenceOrder))
        {
            var referenceValues = rfDef.GetReferenceList();

            foreach (var facet in facetOutput)
            {
                var value = referenceValues.SingleOrDefault(rf => rf.Code == facet.Code);
                if (value != null)
                {
                    value.Count = facet.Count;
                }
                else
                {
                    referenceValues.Add(facet); // A priori si on trouve pas la facette concernée c'est parce que c'est la missing.
                }
            }

            if (!rfDef.ShowEmptyReferenceValues)
            {
                referenceValues = referenceValues.Where(rf => rf.Count > 0).ToList();
            }

            // On est obligé de retrier par derrière.
            switch (facetDef.Ordering)
            {
                case FacetOrdering.ReferenceOrder:
                    return referenceValues;
                case FacetOrdering.CountAscending:
                    return referenceValues.OrderBy(fi => fi.Count).ToList();
                case FacetOrdering.CountDescending:
                    return referenceValues.OrderByDescending(fi => fi.Count).ToList();
                case FacetOrdering.KeyAscending:
                    return referenceValues.OrderBy(fi => fi.Code).ToList();
                case FacetOrdering.KeyDescending:
                    return referenceValues.OrderByDescending(fi => fi.Code).ToList();
            }
        }

        return facetOutput;
    }
}
