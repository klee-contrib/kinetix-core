using System.ComponentModel;
using Kinetix.Search.Models.Annotations;

namespace Kinetix.Search.Core.DocumentModel;

/// <summary>
/// Classe de description d'une propriété.
/// </summary>
public sealed class DocumentFieldDescriptor
{
    /// <summary>
    /// Obtient le nom de la propriété.
    /// </summary>
    public required string PropertyName
    {
        get;
        set;
    }

    /// <summary>
    /// Nom du champ dans le document (camel case).
    /// </summary>
    public required string FieldName
    {
        get;
        set;
    }

    /// <summary>
    /// Obtient le type de la propriété.
    /// </summary>
    public required Type PropertyType
    {
        get;
        set;
    }

    /// <summary>
    /// Catégorie de field de document.
    /// </summary>
    public SearchFieldCategory Category
    {
        get;
        set;
    }

    /// <summary>
    /// Catégorie de field de recherche.
    /// </summary>
    public SearchFieldIndexing Indexing
    {
        get;
        set;
    }

    /// <summary>
    /// Ordre de la propriété dans la clé primaire composite (si applicable).
    /// </summary>
    public int PkOrder
    {
        get;
        set;
    }

    /// <summary>
    /// S'agit-il de la propriété contrôlant le rebuild partiel.
    /// </summary>
    public bool IsPartialRebuildDate
    {
        get;
        set;
    }

    /// <summary>
    /// S'agit-il de la propriété qui peut avoir plusieurs valeurs.
    /// </summary>
    public bool IsMultiValued
    {
        get;
        set;
    }

    /// <summary>
    /// Boost à utiliser sur le champ dans la requête de recherche full-text.
    /// </summary>
    public double Boost
    {
        get;
        set;
    } = 1;

    /// <summary>
    /// Autres attributs sur le champ.
    /// </summary>
    public List<object> OtherAttributes { get; set; } = [];

    /// <summary>
    /// Retourne la valeur de la propriété pour un objet.
    /// </summary>
    /// <param name="bean">Objet.</param>
    /// <returns>Valeur.</returns>
    public object? GetValue(object bean)
    {
        var value = TypeDescriptor.GetProperties(bean)[PropertyName]!.GetValue(bean);
        return value;
    }

    /// <summary>
    /// Retourne une chaîne de caractère représentant l'objet.
    /// </summary>
    /// <returns>Chaîne de caractère représentant l'objet.</returns>
    public override string ToString()
    {
        return FieldName;
    }
}
