namespace Kinetix.Search.Core;

internal interface IIndexingDocumentState
{
}

/// <summary>
/// Contient l'état de réindexation en cours d'un document.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
internal class IndexingDocumentState<TDocument> : IIndexingDocumentState
    where TDocument : class
{
    public HashSet<object> IdsToDelete { get; } = [];
    public HashSet<object> IdsToIndex { get; } = [];
    public bool Reindex { get; set; } = false;

    /// <summary>
    /// Marque un document pour suppression dans son index.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <returns>Succès.</returns>
    public bool RegisterDelete(object id)
    {
        IdsToIndex.Remove(id);
        return IdsToDelete.Add(id);
    }

    /// <summary>
    /// Marque un document pour (ré)indexation.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <returns>Succès.</returns>
    public bool RegisterIndex(object id)
    {
        return !IdsToDelete.Contains(id) && IdsToIndex.Add(id);
    }
}
