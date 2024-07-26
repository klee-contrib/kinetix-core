namespace Kinetix.Search.Models;

/// <summary>
/// Facette en sortie de recherche avancée : liste de facettes.
/// </summary>
public class FacetOutput
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
    /// Libellé de la facette.
    /// </summary>
    public required string Label
    {
        get;
        set;
    }

    /// <summary>
    /// Si la facette est multi sélectionnable.
    /// </summary>
    public bool IsMultiSelectable
    {
        get;
        set;
    }

    /// <summary>
    /// Si le champ facetté peut avoir plusieurs valeurs.
    /// </summary>
    public bool IsMultiValued
    {
        get;
        set;
    }

    /// <summary>
    /// S'il est possible d'exclure des valeurs de facette.
    /// </summary>
    public bool CanExclude
    {
        get;
        set;
    }

    /// <summary>
    /// Valeurs possibles des facettes.
    /// </summary>
    public required ICollection<FacetItem> Values
    {
        get;
        set;
    }
}
