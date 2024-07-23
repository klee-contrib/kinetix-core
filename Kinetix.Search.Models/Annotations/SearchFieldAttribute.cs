namespace Kinetix.Search.Models.Annotations;

/// <summary>
/// Attribut de décoration de propriété de document de moteur de recherche.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SearchFieldAttribute : Attribute
{
    /// <summary>
    /// Créé une nouvelle instance de SearchFieldAttribute.
    /// </summary>
    /// <param name="indexing">Indexage.</param>
    public SearchFieldAttribute(SearchFieldIndexing indexing)
    {
        Indexing = indexing;
    }

    /// <summary>
    /// Créé une nouvelle instance de SearchFieldAttribute.
    /// </summary>
    /// <param name="category">Catégorie.</param>
    public SearchFieldAttribute(SearchFieldCategory category)
    {
        Category = category;

        if (category == SearchFieldCategory.Search)
        {
            Indexing = SearchFieldIndexing.FullText;
        }
        else if (category == SearchFieldCategory.Security)
        {
            Indexing = SearchFieldIndexing.Term;
        }
    }

    /// <summary>
    /// Créé une nouvelle instance de SearchFieldAttribute.
    /// </summary>
    /// <param name="indexing">Indexage.</param>
    /// <param name="category">Catégorie.</param>
    public SearchFieldAttribute(SearchFieldCategory category, SearchFieldIndexing indexing)
    {
        Category = category;
        Indexing = indexing;
    }

    /// <summary>
    /// Créé une nouvelle instance de SearchFieldAttribute.
    /// </summary>
    /// <param name="category">Catégorie.</param>
    /// <param name="indexing">Indexage.</param>
    public SearchFieldAttribute(SearchFieldIndexing indexing, SearchFieldCategory category) : this(category, indexing)
    {
    }

    /// <summary>
    /// Catégorie du champ.
    /// </summary>
    public SearchFieldCategory Category { get; private set; }

    /// <summary>
    /// Type d'indexage du champ.
    /// </summary>
    public SearchFieldIndexing Indexing { get; private set; }

    /// <summary>
    /// Ordre de la propriété dans la clé primaire composite (si applicable).
    /// </summary>
    public int PkOrder { get; set; }

    /// <summary>
    /// Boost à utiliser sur le champ dans la requête de recherche full-text (à utiliser avec SearchFieldCategory.Search).
    /// </summary>
    public double Boost { get; set; }

}
