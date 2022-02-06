﻿using Elasticsearch.Net;
using Kinetix.Monitoring;
using Kinetix.Search.DocumentModel;
using Microsoft.Extensions.Logging;
using Nest;

namespace Kinetix.Search.Elastic;

public class ElasticBulkDescriptor : ISearchBulkDescriptor
{
    private int _operationCount = 0;

    private readonly AnalyticsManager _analytics;
    private readonly BulkDescriptor _bulkDescriptor = new BulkDescriptor()
        .Timeout(TimeSpan.FromMinutes(1))
        .RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMinutes(1)));
    private readonly ElasticClient _client;
    private readonly DocumentDescriptor _documentDescriptor;
    private readonly ILogger<ElasticStore> _logger;

    internal ElasticBulkDescriptor(DocumentDescriptor documentDescriptor, ElasticClient client, ILogger<ElasticStore> logger, AnalyticsManager analytics)
    {
        _analytics = analytics;
        _documentDescriptor = documentDescriptor;
        _client = client;
        _logger = logger;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}(string)" />
    public ISearchBulkDescriptor Delete<TDocument>(string id)
        where TDocument : class
    {
        _bulkDescriptor.Delete<TDocument>(d => d.Id(id));
        _operationCount++;

        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}(TDocument)" />
    public ISearchBulkDescriptor Delete<TDocument>(TDocument bean)
       where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        _bulkDescriptor.Delete<TDocument>(d => d.Id(def.PrimaryKey.GetValue(bean).ToString()));
        _operationCount++;

        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}(IEnumerable{string})" />
    public ISearchBulkDescriptor DeleteMany<TDocument>(IEnumerable<string> ids)
        where TDocument : class
    {
        _bulkDescriptor.DeleteMany<TDocument>(ids, (d, id) => d.Id(id));
        _operationCount++;

        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.Delete{TDocument}(IEnumerable{TDocument})" />
    public ISearchBulkDescriptor DeleteMany<TDocument>(IEnumerable<TDocument> beans)
       where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        _bulkDescriptor.DeleteMany<TDocument>(
            beans.Select(bean => def.PrimaryKey.GetValue(bean).ToString()),
            (d, id) => d.Id(id));
        _operationCount++;

        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.Index" />
    public ISearchBulkDescriptor Index<TDocument>(TDocument document)
        where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        var id = def.PrimaryKey.GetValue(document).ToString();
        _bulkDescriptor.Index<TDocument>(y => y.Document(document).Id(id));
        _operationCount++;

        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.IndexMany" />
    public ISearchBulkDescriptor IndexMany<TDocument>(IList<TDocument> documents)
        where TDocument : class
    {
        var def = _documentDescriptor.GetDefinition(typeof(TDocument));
        _bulkDescriptor.IndexMany(
            documents,
            (b, document) => b.Id(def.PrimaryKey.GetValue(document).ToString()));
        _operationCount++;

        return this;
    }

    /// <inheritdoc cref="ISearchBulkDescriptor.Run" />
    public int Run(bool refresh = true)
    {
        if (_operationCount > 0)
        {
            _logger.LogQuery(_analytics, $"Index {_operationCount}", () =>
                 _client.Bulk(_bulkDescriptor.Refresh(refresh ? Refresh.WaitFor : Refresh.False)));
        }

        return _operationCount;
    }
}
