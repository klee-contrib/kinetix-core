using System.Reflection;
using Kinetix.Search.Models.Annotations;

namespace Kinetix.Search.Core.DocumentModel;

/// <summary>
/// Fournit la description d'un document.
/// </summary>
public class DocumentDescriptor
{
    private readonly Dictionary<Type, DocumentDefinition> _beanDefinitionDictionnary = [];
    private readonly object lockObj = new();

    /// <summary>
    /// Retourne la definition d'un document.
    /// </summary>
    /// <param name="beanType">Type du bean.</param>
    /// <returns>Description des propriétés.</returns>
    public DocumentDefinition GetDefinition(Type beanType)
    {
        return beanType == null
            ? throw new ArgumentNullException(nameof(beanType))
            : GetDefinitionInternal(beanType);
    }

    private IEnumerable<DocumentFieldDescriptor> GetProperties(Type beanType, string? prefix = null, bool isMultiValued = false)
    {
        foreach (var property in beanType.GetProperties())
        {
            var fieldName = ToCamelCase(property.Name);
            var isArray = property.PropertyType.IsArray;
            var propertyType = Nullable.GetUnderlyingType(property.PropertyType)
                ?? (property.PropertyType.IsArray
                    ? property.PropertyType.GetElementType()!
                    : property.PropertyType);

            if (propertyType.GetProperties().Any(prop => prop.GetCustomAttribute<SearchFieldAttribute>() != null))
            {
                foreach (var subProperty in GetProperties(propertyType, prefix != null ? $"{prefix}.{fieldName}" : fieldName, isArray || isMultiValued))
                {
                    yield return subProperty;
                }
            }
            else
            {
                var searchAttr = property.GetCustomAttribute<SearchFieldAttribute>();
                var dateAttr = property.GetCustomAttribute<PartialRebuildDatePropertyAttribute>();

                var description = new DocumentFieldDescriptor
                {
                    PropertyName = property.Name,
                    FieldName = prefix != null ? $"{prefix}.{fieldName}" : fieldName,
                    PropertyType = propertyType,
                    Category = searchAttr?.Category ?? SearchFieldCategory.None,
                    Indexing = searchAttr?.Indexing ?? SearchFieldIndexing.None,
                    PkOrder = searchAttr?.PkOrder ?? 0,
                    IsPartialRebuildDate = dateAttr != null,
                    IsMultiValued = isArray || isMultiValued,
                    Boost = searchAttr?.Boost ?? 1,
                    OtherAttributes = property.GetCustomAttributes(true).Where(a => a is not SearchFieldAttribute and not PartialRebuildDatePropertyAttribute).ToList()
                };

                yield return description.IsPartialRebuildDate && description.PropertyType != typeof(DateTime)
                    ? throw new NotSupportedException($"{beanType}: the {description.FieldName} property must be of type 'DateTime'.")
                    : description;
            }
        }
    }

    /// <summary>
    /// Crée la collection des descripteurs de propriétés.
    /// </summary>
    /// <param name="beanType">Type du bean.</param>
    /// <returns>Collection.</returns>
    private DocumentFieldDescriptorCollection CreateCollection(Type beanType)
    {
        var coll = new DocumentFieldDescriptorCollection(beanType);
        foreach (var description in GetProperties(beanType))
        {
            coll[description.FieldName] = description;
        }

        if (coll.Count(prop => prop.Category == SearchFieldCategory.Security) > 1)
        {
            throw new NotSupportedException($"{beanType} has multiple security properties");
        }

        if (coll.Count(d => d.IsPartialRebuildDate) > 1)
        {
            throw new NotSupportedException($"{beanType} has multiple partial rebuild date properties.");
        }

        return coll;
    }

    /// <summary>
    /// Convertit une chaîne en camelCase.
    /// </summary>
    /// <param name="raw">Chaîne source.</param>
    /// <returns>Chaîne en camelCase.</returns>
    private string ToCamelCase(string raw)
    {
        return string.IsNullOrEmpty(raw) ? raw : char.ToLower(raw[0]) + raw[1..];
    }

    /// <summary>
    /// Retourne la definition d'un bean.
    /// </summary>
    /// <param name="beanType">Type du bean.</param>
    /// <returns>Description des propriétés.</returns>
    private DocumentDefinition GetDefinitionInternal(Type beanType)
    {
        lock (lockObj)
        {
            if (!_beanDefinitionDictionnary.TryGetValue(beanType, out var definition))
            {
                var properties = CreateCollection(beanType);
                definition = new DocumentDefinition(beanType, properties);
                _beanDefinitionDictionnary[beanType] = definition;
            }

            return definition;
        }
    }
}
