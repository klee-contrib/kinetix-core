namespace Kinetix.DataAccess.Sql;

/// <summary>
/// Exception générée par le CollectionBuilder.
/// </summary>
[Serializable]
public class CollectionBuilderException : Exception
{
    /// <summary>
    /// Crée un nouvelle exception.
    /// </summary>
    public CollectionBuilderException()
        : base()
    {
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="message">Description de l'exception.</param>
    public CollectionBuilderException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="message">Description de l'exception.</param>
    /// <param name="innerException">Exception source.</param>
    public CollectionBuilderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
