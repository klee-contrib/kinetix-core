using Kinetix.Search.Core;
using Kinetix.Search.Elastic.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Configurateur pour ElasticSearch.
/// </summary>
public class ElasticConfigBuilder
{
    public const string ServerName = "Elastic6";

    private readonly IServiceCollection _services;

    internal ElasticConfigBuilder(IServiceCollection services)
    {
        _services = services;
        AddMapper<DateTimeMapper>();
        AddMapper<DecimalMapper>();
        AddMapper<IntMapper>();
        AddMapper<StringMapper>();
        AddMapper<DictionaryMapper>();
    }

    internal ICollection<Type> DocumentTypes { get; } = new List<Type>();

    internal ICollection<JsonConverter> JsonConverters { get; } = new List<JsonConverter>();

    /// <summary>
    /// Enregistre un index en lecture pour un document.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    /// <returns>Builder.</returns>
    public ElasticConfigBuilder AddDocumentType<TDocument>()
    {
        DocumentTypes.Add(typeof(TDocument));
        return this;
    }

    /// <summary>
    /// Enregistre un index en lecture et en écriture pour un document.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    /// <typeparam name="TLoader">DocumentLoader pour le document.</typeparam>
    /// <returns>Builder.</returns>
    public ElasticConfigBuilder AddDocumentType<TDocument, TLoader>()
        where TDocument : class
        where TLoader : class, IDocumentLoader<TDocument>
    {
        DocumentTypes.Add(typeof(TDocument));
        _services.AddScoped<IDocumentLoader<TDocument>, TLoader>();
        return this;
    }

    /// <summary>
    /// Ajoute un mapping pour un type de champ.
    /// </summary>
    /// <typeparam name="TMapper">Type de champ.</typeparam>
    /// <returns>Builder.</returns>
    public ElasticConfigBuilder AddMapper<TMapper>()
        where TMapper : class, IElasticMapper
    {
        _services.AddSingleton(typeof(TMapper).GetInterfaces().First(), typeof(TMapper));
        return this;
    }

    /// <summary>
    /// Ajoute un converter Json pour un type de champ.
    /// </summary>
    /// <typeparam name="TJsonConverter">JsonConverter</typeparam>
    /// <returns>Builder.</returns>
    public ElasticConfigBuilder AddJsonConverter<TJsonConverter>()
        where TJsonConverter : JsonConverter, new()
    {
        JsonConverters.Add(new TJsonConverter());
        return this;
    }
}
