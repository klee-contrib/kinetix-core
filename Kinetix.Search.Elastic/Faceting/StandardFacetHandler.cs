﻿using System.Collections.Generic;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.MetaModel;
using Kinetix.Search.Model;
using Nest;

namespace Kinetix.Search.Elastic.Faceting
{
    /// <summary>
    /// Handler de facette standard.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public class StandardFacetHandler<TDocument> : IFacetHandler<TDocument>
        where TDocument : class
    {
        private const string MissingFacetPrefix = "_Missing";
        private readonly DocumentDefinition _document;
        private readonly ElasticQueryBuilder _builder = new ElasticQueryBuilder();

        /// <summary>
        /// Créé une nouvelle instance de StandardFacetHandler.
        /// </summary>
        /// <param name="document">Définition du document.</param>
        public StandardFacetHandler(DocumentDefinition document)
        {
            _document = document;
        }

        /// <inheritdoc/>
        public void DefineAggregation(Nest.AggregationContainerDescriptor<TDocument> agg, IFacetDefinition facet, ICollection<IFacetDefinition> facetList, FacetListInput selectedFacets, string portfolio)
        {
            /* Récupère le nom du champ. */
            var fieldName = _document.Fields[facet.FieldName].FieldName;

            /* On construit la requête de filtrage sur les autres facettes multi-sélectionnables. */
            var filterQuery = FacetingUtil.BuildMultiSelectableFacetFilter(_builder, facet, facetList, selectedFacets, CreateFacetSubQuery);
            var hasFilterQuery = !string.IsNullOrEmpty(filterQuery);

            ITermsAggregation GetAgg(TermsAggregationDescriptor<TDocument> st)
            {
                return st
                    .Field(fieldName)
                    .Size(50)
                    .Order(t =>
                    {
                        switch (facet.Ordering)
                        {
                            case FacetOrdering.KeyAscending:
                                return t.KeyAscending();
                            case FacetOrdering.KeyDescending:
                                return t.KeyDescending();
                            case FacetOrdering.CountAscending:
                                return t.CountAscending();
                            default:
                                return t.CountDescending();
                        }
                    });
            }

            if (!hasFilterQuery)
            {
                /* Crée une agrégation sur les valeurs discrètes du champ. */
                agg.Terms(facet.Code, GetAgg);

                /* Crée une agrégation pour les valeurs non renseignées du champ. */
                if (facet.HasMissing)
                {
                    agg.Missing(facet.Code + MissingFacetPrefix, ad => ad.Field(fieldName));
                }
            }
            else
            {
                agg.Filter(facet.Code, f => f
                    /* Crée le filtre sur les facettes multi-sélectionnables. */
                    .Filter(q => q.QueryString(qs => qs.Query(filterQuery)))
                    .Aggregations(aa =>
                    {
                        /* Crée une agrégation sur les valeurs discrètes du champ. */
                        aa.Terms(facet.Code, GetAgg);

                        /* Crée une agrégation pour les valeurs non renseignées du champ. */
                        if (facet.HasMissing)
                        {
                            aa.Missing(facet.Code + MissingFacetPrefix, ad => ad.Field(fieldName));
                        }

                        return aa;
                    }));
            }
        }

        /// <inheritdoc />
        public ICollection<FacetItem> ExtractFacetItemList(Nest.AggregateDictionary aggs, IFacetDefinition facetDef, long total)
        {
            var facetOutput = new List<FacetItem>();

            /* Valeurs renseignées. */
            var bucket = aggs.Terms(facetDef.Code);
            if (bucket == null)
            {
                bucket = aggs.Filter(facetDef.Code).Terms(facetDef.Code);
            }

            foreach (var b in bucket.Buckets)
            {
                facetOutput.Add(new FacetItem { Code = b.Key, Label = facetDef.ResolveLabel(b.Key), Count = b.DocCount ?? 0 });
            }

            /* Valeurs non renseignées. */
            if (facetDef.HasMissing)
            {
                var missingBucket = aggs.Missing(facetDef.Code + MissingFacetPrefix);
                if (missingBucket == null)
                {
                    missingBucket = aggs.Filter(facetDef.Code).Missing(facetDef.Code + MissingFacetPrefix);
                }

                var missingCount = missingBucket.DocCount;
                if (missingCount > 0)
                {
                    facetOutput.Add(new FacetItem { Code = FacetConst.NullValue, Label = "focus.search.results.missing", Count = missingCount });
                }
            }

            return facetOutput;
        }

        /// <inheritdoc/>
        public void CheckFacet(IFacetDefinition facetDef)
        {
            if (!_document.Fields.HasProperty(facetDef.FieldName))
            {
                throw new ElasticException("The Document \"" + _document.DocumentTypeName + "\" is missing a \"" + facetDef.FieldName + "\" property to facet on.");
            }
        }

        /// <inheritdoc/>
        public string CreateFacetSubQuery(string facet, IFacetDefinition facetDef, string portfolio)
        {
            var fieldDesc = _document.Fields[facetDef.FieldName];
            var fieldName = fieldDesc.FieldName;

            /* Traite la valeur de sélection NULL */
            return facet == FacetConst.NullValue
                ? _builder.BuildMissingField(fieldName)
                : _builder.BuildFilter(fieldName, facet);
        }
    }
}
