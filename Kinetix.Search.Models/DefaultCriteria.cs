namespace Kinetix.Search.Models;

/// <summary>
/// Implémentation par défaut du critère de recherche.
/// </summary>
public class DefaultCriteria : ICriteria
{
    /// <summary>
    /// Critère de recherche.
    /// </summary>
    public string? Query
    {
        get;
        set;
    }

    /// <summary>
    /// Liste des champs sur lesquels rechercher.
    /// </summary>
    public IList<string>? SearchFields
    {
        get;
        set;
    }

    /// <summary>
    /// Liste des champs à inclure dans la recherche ES.
    /// Si non renseigné (ou vide) : tous les champs seront inclus.
    /// </summary>
    public IList<string>? SourceFields
    {
        get;
        set;
    }
}
