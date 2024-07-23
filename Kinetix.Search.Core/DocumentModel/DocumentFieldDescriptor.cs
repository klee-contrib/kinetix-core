using System.ComponentModel;
using Kinetix.Search.Models.Annotations;

namespace Kinetix.Search.Core.DocumentModel;

/// <summary>
/// Classe de description d'une propriété.
/// </summary>
[Serializable]
public sealed class DocumentFieldDescriptor
{
    /// <summary>
    /// Obtient le nom de la propriété.
    /// </summary>
    public string PropertyName
    {
        get;
        internal set;
    }

    /// <summary>
    /// Nom du champ dans le document (camel case).
    /// </summary>
    public string FieldName
    {
        get;
        internal set;
    }

    /// <summary>
    /// Obtient le type de la propriété.
    /// </summary>
    public Type PropertyType
    {
        get;
        internal set;
    }

    /// <summary>
    /// Catégorie de field de document.
    /// </summary>
    public SearchFieldCategory Category
    {
        get;
        internal set;
    }

    /// <summary>
    /// Catégorie de field de recherche.
    /// </summary>
    public SearchFieldIndexing Indexing
    {
        get;
        internal set;
    }

    /// <summary>
    /// Ordre de la propriété dans la clé primaire composite (si applicable).
    /// </summary>
    public int PkOrder
    {
        get;
        internal set;
    }

    /// <summary>
    /// S'agit-il de la propriété contrôlant le rebuild partiel.
    /// </summary>
    public bool IsPartialRebuildDate
    {
        get;
        internal set;
    }

    /// <summary>
    /// S'agit-il de la propriété qui peut avoir plusieurs valeurs.
    /// </summary>
    public bool IsMultiValued
    {
        get;
        internal set;
    }

    /// <summary>
    /// Boost à utiliser sur le champ dans la requête de recherche full-text.
    /// </summary>
    public decimal Boost
    {
        get;
        internal set;
    } = 1;

    /// <summary>
    /// Autres attributs sur le champ.
    /// </summary>
    public List<object> OtherAttributes { get; set; }

    /// <summary>
    /// Retourne la valeur de la propriété pour un objet.
    /// </summary>
    /// <param name="bean">Objet.</param>
    /// <returns>Valeur.</returns>
    public object GetValue(object bean)
    {
        var value = TypeDescriptor.GetProperties(bean)[PropertyName].GetValue(bean);
        return value;
    }

    /// <summary>
    /// Définit la valeur de la propriété pour un objet.
    /// </summary>
    /// <param name="bean">Objet.</param>
    /// <param name="value">Valeur.</param>
    public void SetValue(object bean, object value)
    {
        var descriptor = TypeDescriptor.GetProperties(bean)[PropertyName];
        descriptor.SetValue(bean, value);
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
