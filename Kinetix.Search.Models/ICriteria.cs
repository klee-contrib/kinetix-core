namespace Kinetix.Search.Models;

/// <summary>
/// Critère de recherche.
/// </summary>
public interface ICriteria
{
    /// <summary>
    /// Critère de recherche.
    /// </summary>
    string? Query { get; set; }

    /// <summary>
    /// Liste des champs sur lesquels rechercher.
    /// </summary>
    IList<string>? SearchFields { get; set; }

    /// <summary>
    /// Liste des champs à inclure dans la recherche ES.
    /// Si non renseigné (ou vide) : tous les champs seront inclus.
    /// </summary>
    IList<string>? SourceFields { get; set; }
}