using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Elastic.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Usine à mapping ElasticSearch.
/// </summary>
public sealed class ElasticMappingFactory
{
    private readonly IServiceProvider _provider;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="provider"></param>
    public ElasticMappingFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Effectue le mapping pour les champs d'un document.
    /// </summary>
    /// <param name="selector">Descripteur des propriétés.</param>
    /// <param name="fields">Les champs.</param>
    /// <returns>Mapping de champ.</returns>
    /// <typeparam name="T">Type du document.</typeparam>
    public PropertiesDescriptor<T> AddFields<T>(PropertiesDescriptor<T> selector, DocumentFieldDescriptorCollection fields)
         where T : class
    {
        foreach (var field in fields.OrderBy(field => field.FieldName))
        {
            AddField(selector, field);
        }

        return selector;
    }

    /// <summary>
    /// Effectue le mapping pour un champ d'un document.
    /// </summary>
    /// <param name="selector">Descripteur des propriétés.</param>
    /// <param name="field">Le champ.</param>
    /// <returns>Mapping de champ.</returns>
    /// <typeparam name="T">Type du document.</typeparam>
    public PropertiesDescriptor<T> AddField<T>(PropertiesDescriptor<T> selector, DocumentFieldDescriptor field)
        where T : class
    {
        var mapperType = field.OtherAttributes.OfType<ElasticMapperAttribute>().FirstOrDefault()?.MapperType;

        if (mapperType != null)
        {
            return ((IElasticMapper)Activator.CreateInstance(mapperType)!).Map(selector, field);
        }

        if (_provider.GetService(typeof(IElasticMapper<>).MakeGenericType(field.PropertyType)) is not IElasticMapper mapper)
        {
            mapper = _provider.GetRequiredService<IElasticMapper<string>>();
        }

        return mapper.Map(selector, field);
    }
}
