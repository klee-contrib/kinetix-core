using System.Reflection;
using Kinetix.Search.Models.Annotations;

namespace Kinetix.Search.Core.DocumentModel;

/// <summary>
/// Définition d'un bean.
/// </summary>
[Serializable]
public class DocumentDefinition
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="beanType">Type du bean.</param>
    /// <param name="properties">Collection de propriétés.</param>
    internal DocumentDefinition(Type beanType, DocumentFieldDescriptorCollection properties)
    {
        BeanType = beanType;
        Fields = properties;
        IgnoreOnPartialRebuild = beanType.GetCustomAttribute<IgnoreOnPartialRebuildAttribute>();
        foreach (var property in properties)
        {
            switch (property.Category)
            {
                case SearchFieldCategory.Id:
                    PrimaryKey.AddProperty(property);
                    break;
                case SearchFieldCategory.Search:
                    SearchFields.Add(property);
                    break;
                case SearchFieldCategory.Security:
                    SecurityField = property;
                    break;
                default:
                    break;
            }

            if (property.IsPartialRebuildDate)
            {
                PartialRebuildDate = property;
            }
        }

        if ((IgnoreOnPartialRebuild?.OlderThanDays ?? 0) > 0 && PartialRebuildDate == null)
        {
            throw new NotSupportedException($"{beanType} must have a partial rebuild date property if 'OlderThanDays' > 0.");
        }
    }

    /// <summary>
    /// Retourne le type du bean.
    /// </summary>
    public Type BeanType
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne la clef primaire si elle existe.
    /// </summary>
    public DocumentPrimaryKeyDescriptor PrimaryKey
    {
        get;
        private set;
    } = new();

    /// <summary>
    /// Retourne les propriétés de recherche textuelle.
    /// </summary>
    public ICollection<DocumentFieldDescriptor> SearchFields
    {
        get;
        private set;
    } = [];

    /// <summary>
    /// Retourne la propriété de filtrage de sécurité.
    /// </summary>
    public DocumentFieldDescriptor? SecurityField
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne la liste des propriétés d'un bean.
    /// </summary>
    public DocumentFieldDescriptorCollection Fields
    {
        get;
        private set;
    }

    /// <summary>
    /// Précise si le document a une condition de rebuild partiel.
    /// </summary>
    public IgnoreOnPartialRebuildAttribute? IgnoreOnPartialRebuild
    {
        get;
        private set;
    }

    /// <summary>
    /// Retourne la propriété de date qui contrôle le rebuild partiel.
    /// </summary>
    public DocumentFieldDescriptor? PartialRebuildDate
    {
        get;
        private set;
    }
}
