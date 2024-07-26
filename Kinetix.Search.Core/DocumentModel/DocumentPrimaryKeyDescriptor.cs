using System.Runtime.CompilerServices;

namespace Kinetix.Search.Core.DocumentModel;

/// <summary>
/// Fournit la description de la clé primaire d'un document.s
/// </summary>
public class DocumentPrimaryKeyDescriptor
{
    private List<DocumentFieldDescriptor> _fieldDescriptors = [];

    /// <summary>
    /// Ajoute une propriété pour construire la clé primaire.
    /// </summary>
    /// <param name="field">La propriété.</param>
    public void AddProperty(DocumentFieldDescriptor field)
    {
        _fieldDescriptors.Add(field);
        _fieldDescriptors = _fieldDescriptors.OrderBy(a => a.PkOrder).ThenBy(a => a.FieldName).ToList();
    }

    /// <summary>
    /// Récupère la valeur de la clé primaire d'un document.
    /// </summary>
    /// <param name="document">Le document.</param>
    /// <returns>La clé primaire.</returns>
    public string GetValueFromDocument(object document)
    {
        return _fieldDescriptors.Count > 1
            ? string.Join("__", _fieldDescriptors.Select(f => f.GetValue(document)).Where(v => v != null))
            : _fieldDescriptors.Single().GetValue(document)!.ToString()!;
    }

    /// <summary>
    /// Récupère la valeur de la clé primaire à partir de l'objet de clé.
    /// </summary>
    /// <param name="key">Le clé.</param>
    /// <returns>La clé primaire.</returns>
    public string GetValueFromKeyObject(object key)
    {
        if (_fieldDescriptors.Count == 1)
        {
            return key.ToString()!;
        }

        if (key is not ITuple tuple || tuple.Length > _fieldDescriptors.Count)
        {
            throw new InvalidOperationException("La clé composite du document doit être un tuple.");
        }

        if (_fieldDescriptors.Any(f => f.PkOrder == 0))
        {
            throw new InvalidOperationException("La propriété `PkOrder` doit être renseignée sur les ids de clé composite (à partir de 1).");
        }

        return string.Join("__", _fieldDescriptors.Select(f => tuple.Length < f.PkOrder - 1 ? tuple[f.PkOrder - 1] : null).Where(v => v != null));
    }
}
