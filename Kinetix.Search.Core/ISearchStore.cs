using Kinetix.Search.Core.Querying;
using Kinetix.Search.Models;
using Microsoft.Extensions.Logging;

namespace Kinetix.Search.Core;

/// <summary>
/// Contrat des stores de recherche.
/// </summary>
public interface ISearchStore
{
    /// <summary>
    /// Permet d'effectuer des indexations et de suppressions en masse.
    /// </summary>
    /// <returns>ISearchBulkDescriptor.</returns>
    ISearchBulkDescriptor Bulk();

    /// <summary>
    /// Supprime un document de l'index.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <param name="refresh">Attends ou non la réindexation.</param>
    void Delete<TDocument>(object id, bool refresh = true)
        where TDocument : class;

    /// <summary>
    /// S'assure que l'index existe, avec le mapping à jour.
    /// </summary>
    /// <returns>True si l'index a été (re)créé.</returns>
    bool EnsureIndex<TDocument>()
        where TDocument : class;

    /// <summary>
    /// Obtient un document à partir de son ID.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <returns>Document.</returns>
    TDocument Get<TDocument>(object id)
        where TDocument : class;

    /// <summary>
    /// Pose un document dans l'index.
    /// </summary>
    /// <param name="document">Document à poser.</param>
    /// <param name="refresh">Attends ou non la réindexation.</param>
    void Index<TDocument>(TDocument document, bool refresh = true)
        where TDocument : class;

    /// <summary>
    /// Réinitialise l'index avec les documents fournis.
    /// </summary>
    /// <param name="documents">Documents.</param>
    /// <param name="partialRebuild">Reconstruction partielle (si un index à jour existe déjà).</param>
    /// <param name="rebuildLogger">Logger custom pour suivre l'avancement de la réindexation.</param>
    /// <returns>Le nombre de documents.</returns>
    int ResetIndex<TDocument>(IEnumerable<TDocument> documents, bool partialRebuild, ILogger? rebuildLogger = null)
        where TDocument : class;

    /// <summary>
    /// Effectue une recherche avancée.
    /// </summary>
    /// <param name="input">Entrée de la recherche.</param>
    /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
    /// <returns>Sortie de la recherche.</returns>
    QueryOutput<TOutput> AdvancedQuery<TDocument, TOutput, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper)
        where TDocument : class
        where TCriteria : ICriteria;

    /// <summary>
    /// Effectue une recherche avancée.
    /// </summary>
    /// <param name="input">Entrée de la recherche.</param>
    /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
    /// <returns>Sortie de la recherche.</returns>
    QueryOutput<TOutput> AdvancedQuery<TDocument, TOutput, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper)
        where TDocument : class
        where TCriteria : ICriteria;

    /// <summary>
    /// Effectue une recherche avancée mutiple.
    /// </summary>
    /// <returns>Descripteur.</returns>
    IMultiAdvancedQueryDescriptor MultiAdvancedQuery();

    /// <summary>
    /// Effectue un count avancé.
    /// </summary>
    /// <param name="input">Entrée de la recherche.</param>
    /// <returns>Nombre de documents.</returns>
    long AdvancedCount<TDocument, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
        where TDocument : class
        where TCriteria : ICriteria;
}
