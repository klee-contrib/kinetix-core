namespace Kinetix.Search.Models;

/// <summary>
/// Liste de groupe de résultats lors d'une recherche par groupe.
/// Association valeur du champ de groupe => liste de résultats.
/// </summary>
/// <typeparam name="TDocument">Type du document.</typeparam>
public class GroupResult<TDocument>
{
    /// <summary>
    /// Code du groupe (= celui de la facette).
    /// </summary>
    public required string Code
    {
        get;
        set;
    }

    /// <summary>
    /// Label du groupe (= celui de la facette)
    /// </summary>
    public required string Label
    {
        get;
        set;
    }

    /// <summary>
    /// Liste d'éléments du groupe.
    /// </summary>
    public required ICollection<TDocument> List
    {
        get;
        set;
    }

    /// <summary>
    /// Nombre d'éléments du groupe.
    /// </summary>
    public required int TotalCount
    {
        get;
        set;
    }
}
