namespace Kinetix.Search.Models;

/// <summary>
/// Item de facette.
/// </summary>
public class FacetItem
{
    /// <summary>
    /// Code de la facette.
    /// </summary>
    public required string Code
    {
        get;
        set;
    }

    /// <summary>
    /// Libellé de l'item.
    /// </summary>
    public required string Label
    {
        get;
        set;
    }

    /// <summary>
    /// Nombre d'éléments pour l'item.
    /// </summary>
    public required long Count
    {
        get;
        set;
    }
}
