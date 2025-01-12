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
    /// <param name="config">Configuration.</param>
    /// <returns>Services.</returns>
    public static IServiceCollection AddKinetixExceptionHandler(this IServiceCollection services, KinetixExceptionConfig? config = null)
    {
        return services
            .AddProblemDetails(o =>
            {
                if (config?.CustomizeProblemDetails != null)
                {
                    o.CustomizeProblemDetails = config.CustomizeProblemDetails;
                }
            })
            .AddSingleton(config ?? new())
            .AddExceptionHandler<KinetixExceptionHandler>()
            .AddSingleton<IKinetixExceptionHandler, BusinessExceptionHandler>()
            .AddSingleton<IKinetixExceptionHandler, MissingEntityExceptionHandler>()
            .AddSingleton<IKinetixExceptionHandler, SecurityExceptionHandler>();
    }

    /// <summary>
    /// Sérialise les erreurs de validation MVC comme des KinetixErrorResponse.<br /><br />
    /// A utiliser avec un KinetixExceptionHandler configuré avec (la sérialisation par défaut utilise des ProblemDetails).
    /// </summary>
    /// <param name="builder">MvcBuilder.</param>
    /// <returns>MvcBuilder</returns>
    public static IMvcBuilder SerializeInvalidModelStateAsKinetixErrorResponse(this IMvcBuilder builder)
    {
        return builder.ConfigureApiBehaviorOptions(o =>
        {
            o.InvalidModelStateResponseFactory = context =>
                 new JsonResult(new KinetixErrorResponse
                 {
                     Errors = context.ModelState.SelectMany(field => (field.Value?.Errors ?? []).Select(error => $"{field.Key}: {error.ErrorMessage}")).ToArray()
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
