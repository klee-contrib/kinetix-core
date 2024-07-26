namespace Kinetix.Search.Core;

/// <summary>
/// Permet de réaliser des indexations et suppressions en masse.
/// </summary>
public interface ISearchBulkDescriptor
{
    /// <summary>
    /// Supprime un document d'un index.
    /// </summary>
    /// <typeparam name="TDocument">Type de document.</typeparam>
    /// <param name="key">La clé composite.</param>
    /// <returns>ISearchBulkDescriptor.</returns>
    ISearchBulkDescriptor Delete<TDocument>(object key)
        where TDocument : class;

    /// <summary>
    /// Supprime des document d'un index.
    /// </summary>
    /// <typeparam name="TDocument">Type de document.</typeparam>
    /// <param name="keys">Les clés composites.</param>
    /// <returns>ISearchBulkDescriptor.</returns>
    ISearchBulkDescriptor DeleteMany<TDocument>(IEnumerable<object> keys)
        where TDocument : class;

    /// <summary>
    /// Pose un document dans un index.
    /// </summary>
    /// <typeparam name="TDocument">Type de document.</typeparam>
    /// <param name="document">Document à poser.</param>
    /// <returns>ISearchBulkDescriptor.</returns>
    ISearchBulkDescriptor Index<TDocument>(TDocument document)
        where TDocument : class;

    /// <summary>
    /// Pose des documents dans un index.
    /// </summary>
    /// <typeparam name="TDocument">Type de document.</typeparam>
    /// <param name="documents">Documents à poser.</param>
    /// <returns>ISearchBulkDescriptor.</returns>
    ISearchBulkDescriptor IndexMany<TDocument>(IList<TDocument> documents)
        where TDocument : class;

    /// <summary>
    /// Effectue la requête.
    /// </summary>
    /// <param name="refresh">Attends ou non la réindexation.</param>
    /// <returns>Le nombre de documents indexés et supprimés.</returns>
    int Run(bool refresh = true);
}
