namespace Kinetix.Search.Elastic;

/// <summary>
/// Exception générée par les appels à ElasticSearch.
/// </summary>
[Serializable]
public class ElasticException : Exception
{
    /// <summary>
    /// Détails de l'appel ElasticSearch.
    /// </summary>
    public string? DebugInformation { get; set; }

    /// <summary>
    /// Crée un nouvelle exception.
    /// </summary>
    public ElasticException()
    {
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="message">Description de l'exception.</param>
    public ElasticException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="message">Description de l'exception.</param>
    /// <param name="debugInformation">Détails de l'appel ElasticSearch.</param>
    /// <param name="originalException">Exception originale.</param>
    public ElasticException(string message, string debugInformation, Exception originalException)
        : base(message, originalException)
    {
        DebugInformation = debugInformation;
    }
}
