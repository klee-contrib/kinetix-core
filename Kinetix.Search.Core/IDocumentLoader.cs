namespace Kinetix.Search.Core;

/// <summary>
/// Contrat pour les loaders de documents pour indexation.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
public interface IDocumentLoader<TDocument>
    where TDocument : class
{
    /// <summary>
    /// Charge un document pour indexation.
    /// </summary>
    /// <param name="id">Id du document.</param>
    /// <returns>Le document.</returns>
    TDocument Get(object id);

    /// <summary>
    /// Charge tous les documents pour indexation.
    /// </summary>
    /// <param name="partialRebuild">Indique que l'on veut un rebuild partiel, donc certains documents peuvent être ignorés.</param>
    /// <returns>Les documents.</returns>
    IEnumerable<TDocument> GetAll(bool partialRebuild);

    /// <summary>
    /// Charge plusieurs documents pour indexation.
    /// </summary>
    /// <param name="ids">Ids des documents.</param>
    /// <returns>Les documents.</returns>
    IEnumerable<TDocument> GetMany(IEnumerable<object> ids);
}

/// <summary>
/// Implé abstraite de IDocumentLoader avec la clé primaire typée.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
/// <typeparam name="TKey">Type de la clé primaire. Si la clé est composite, alors le type doit être un tuple avec les propriétés dans le bon ordre.</typeparam>
public abstract class DocumentLoader<TDocument, TKey> : IDocumentLoader<TDocument>
    where TDocument : class
{
    /// <inheritdoc cref="IDocumentLoader{TDocument}.Get" />
    public TDocument Get(object id)
    {
        return Get((TKey)id);
    }

    /// <summary>
    /// Charge un document pour indexation.
    /// </summary>
    /// <param name="id">Id du document.</param>
    /// <returns>Le document.</returns>
    public abstract TDocument Get(TKey id);

    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetAll" />
    public abstract IEnumerable<TDocument> GetAll(bool partialRebuild);

    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetMany" />
    public IEnumerable<TDocument> GetMany(IEnumerable<object> ids)
    {
        return GetMany(ids.Cast<TKey>());
    }

    /// <summary>
    /// Charge plusieurs documents pour indexation.
    /// </summary>
    /// <param name="ids">Ids des documents.</param>
    /// <returns>Les documents.</returns>
    public abstract IEnumerable<TDocument> GetMany(IEnumerable<TKey> ids);
}
