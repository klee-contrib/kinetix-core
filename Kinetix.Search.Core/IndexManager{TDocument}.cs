using Kinetix.Search.Core.Config;
using Kinetix.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetix.Search.Core;

/// <summary>
/// IndexManager pour un document.
/// </summary>
/// <typeparam name="TDocument"></typeparam>
public class IndexManager<TDocument>
    where TDocument : class
{
    private readonly ILogger<IndexManager> _logger;
    private readonly IServiceProvider _provider;
    private readonly ISearchStore _searchStore;
    private readonly TransactionScopeManager _transactionScopeManager;
    private readonly bool _waitForRefresh;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="provider">Composant injecté.</param>
    /// <param name="searchStore">Composant injecté.</param>
    /// <param name="transactionScopeManager">Composant injecté.</param>
    /// <param name="waitForRefresh">WaitForRefresh/</param>
    internal IndexManager(ILogger<IndexManager> logger, IServiceProvider provider, ISearchStore searchStore, TransactionScopeManager transactionScopeManager, bool waitForRefresh)
    {
        _logger = logger;
        _provider = provider;
        _searchStore = searchStore;
        _transactionScopeManager = transactionScopeManager;
        _waitForRefresh = waitForRefresh;
    }

    /// <summary>
    /// Marque un document pour suppression dans son index.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <returns>IndexManager.</returns>
    public IndexManager<TDocument> Delete<TKey>(TKey id)
        where TKey : notnull
    {
        _logger.LogInformation($"RegisterDelete 1 {typeof(TDocument).Name}");
        GetContext().RegisterDelete<TDocument>(id);
        return this;
    }

    /// <summary>
    /// Marque plusieurs documents pour suppression dans leur index.
    /// </summary>
    /// <param name="ids">IDs des documents.</param>
    /// <returns>IndexManager.</returns>
    public IndexManager<TDocument> DeleteMany<TKey>(IEnumerable<TKey> ids)
        where TKey : notnull
    {
        _logger.LogInformation($"RegisterDelete {ids.Count()} {typeof(TDocument).Name}");
        foreach (var id in ids)
        {
            GetContext().RegisterDelete<TDocument>(id);
        }

        return this;
    }

    /// <summary>
    /// Marque un document pour (ré)indexation.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <returns>IndexManager.</returns>
    public IndexManager<TDocument> Index<TKey>(TKey id)
        where TKey : notnull
    {
        _logger.LogInformation($"RegisterIndex 1 {typeof(TDocument).Name}");
        GetContext().RegisterIndex<TDocument>(id);
        return this;
    }

    /// <summary>
    /// Marque plusieurs documents pour ré(indexation).
    /// </summary>
    /// <param name="ids">IDs des documents.</param>
    /// <returns>IndexManager.</returns>
    public IndexManager<TDocument> IndexMany<TKey>(IEnumerable<TKey> ids)
        where TKey : notnull
    {
        _logger.LogInformation($"RegisterIndex {ids.Count()} {typeof(TDocument).Name}");
        foreach (var id in ids)
        {
            GetContext().RegisterIndex<TDocument>(id);
        }

        return this;
    }

    /// <summary>
    /// Réinitialise un index.
    /// </summary>
    public IndexManager<TDocument> IndexAll()
    {
        _logger.LogInformation($"Reindex {typeof(TDocument).Name}");
        GetContext().IndexAll<TDocument>();
        return this;
    }

    /// <summary>
    /// Reconstruit un index.
    /// </summary>
    /// <param name="rebuildLogger">Logger custom pour suivre l'avancement de la réindexation.</param>
    /// <returns>Le nombre de documents.</returns>
    public int RebuildIndex(ILogger? rebuildLogger = null)
    {
        using var tx = _transactionScopeManager.EnsureTransaction();

        var indexName = SearchConfig.GetTypeNameForIndex(typeof(TDocument));

        rebuildLogger?.LogInformation($"Index {indexName} rebuild started...");
        var indexCreated = _searchStore.EnsureIndex<TDocument>();
        if (indexCreated)
        {
            rebuildLogger?.LogInformation($"Index {indexName} (re)created.");
        }

        rebuildLogger?.LogInformation($"Loading data for index {indexName}...");

        var loader = _provider.GetRequiredService<IDocumentLoader<TDocument>>();

        var documents = loader.GetAll(!indexCreated);
        rebuildLogger?.LogInformation($"Data for index {indexName} loaded.");

        return _searchStore.ResetIndex(documents, !indexCreated, rebuildLogger);
    }

    private IndexingTransactionContext GetContext()
    {
        var context = _transactionScopeManager.ActiveScope?.GetContext<IndexingTransactionContext>();

        if (context != null)
        {
            context.WaitForRefresh = _waitForRefresh;
            return context;
        }

        throw new InvalidOperationException("Impossible d'enregistrer une réindexation en dehors d'un contexte de transaction.");
    }
}