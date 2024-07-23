namespace Kinetix.DataAccess.Sql.Broker;

/// <summary>
/// Exception générée par le broker.
/// </summary>
[Serializable]
public class BrokerException : Exception
{
    /// <summary>
    /// Crée un nouvelle exception.
    /// </summary>
    public BrokerException()
        : base()
    {
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="message">Description de l'exception.</param>
    public BrokerException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Crée une nouvelle exception.
    /// </summary>
    /// <param name="message">Description de l'exception.</param>
    /// <param name="innerException">Exception source.</param>
    public BrokerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
