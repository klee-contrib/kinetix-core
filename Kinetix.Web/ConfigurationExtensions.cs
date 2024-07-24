using System.Text.Json;
using System.Text.Json.Serialization;
using Kinetix.Web.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Web;

/// <summary>
/// Extensions pour configurer Kinetix.Web.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Ajoute un convertisseur JSON dans les options.
    /// </summary>
    /// <param name="options">JSONOptions.</param>
    public static JsonSerializerOptions AddConverter<T>(this JsonSerializerOptions options)
        where T : JsonConverter, new()
    {
        options.Converters.Add(new T());
        return options;
    }

    /// <summary>
    /// Enregistre un filtre de type <typeparamref name="TFilter" /> sur tous les contrôleurs.
    /// </summary>
    /// <param name="builder">Builder d'endpoint de contrôleurs.</param>
    public static ControllerActionEndpointConventionBuilder AddEndpointFilter<TFilter>(this ControllerActionEndpointConventionBuilder builder)
        where TFilter : IEndpointFilter
    {
        return builder.AddEndpointFilter<ControllerActionEndpointConventionBuilder, TFilter>();
    }

    /// <summary>
    /// Enregistre la gestion d'exception de Kinetix.<br /><br />
    /// Vous pouvez créer vos propres handlers d'exception en enregistrant un service qui implémente <see cref="IKinetixExceptionHandler" />
    /// </summary>
    /// <param name="services">Services.</param>
    /// <returns>Services.</returns>
    public static IServiceCollection AddKinetixExceptionHandler(this IServiceCollection services)
    {
        return services
            .AddProblemDetails()
            .AddExceptionHandler<KinetixExceptionHandler>()
            .AddSingleton<IKinetixExceptionHandler, BusinessExceptionHandler>()
            .AddSingleton<IKinetixExceptionHandler, MissingEntityExceptionHandler>()
            .AddSingleton<IKinetixExceptionHandler, SecurityExceptionHandler>();
    }

    /// <summary>
    /// Configure la sérialisation des erreurs de validation MVC pour être raccord avec les exceptions remontées.
    /// </summary>
    /// <param name="builder">MvcBuilder.</param>
    /// <returns>MvcBuilder</returns>
    public static IMvcBuilder ConfigureInvalidModelStateSerialization(this IMvcBuilder builder)
    {
        return builder.ConfigureApiBehaviorOptions(o =>
        {
            o.InvalidModelStateResponseFactory = context =>
                 new JsonResult(new KinetixErrorResponse
                 {
                     Errors = context.ModelState.SelectMany(ms => ms.Value?.Errors ?? []).Select(e => e.ErrorMessage).ToArray()
                 })
                 { StatusCode = 400 };
        });
    }

    /// <summary>
    /// Configuration par défaut pour la sérialisation JSON.
    /// </summary>
    /// <param name="options">JSONOptions.</param>
    public static JsonSerializerOptions ConfigureSerializerDefaults(this JsonSerializerOptions options)
    {
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.AddConverter<DateTimeConverter>();
        options.AddConverter<TimeSpanConverter>();
        return options;
    }
}
