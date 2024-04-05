using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;

namespace Kinetix.Monitoring.Insights;

/// <summary>
/// Intercepte les logs d'EF Core de requête SQL pour les transformer en DependencyTelemetry
/// </summary>
public class DbCommandTelemetryProcessor : ITelemetryProcessor
{
    private readonly string _databaseName;
    private readonly ITelemetryProcessor _next;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="next">Processor suivant.</param>
    /// <param name="config">Composant injecté.</param>
    public DbCommandTelemetryProcessor(ITelemetryProcessor next, IConfiguration config)
    {
        _next = next;

        var db = new DbConnectionStringBuilder { ConnectionString = config.GetConnectionString("default") };
        _databaseName = $"{(string)db["Database"]}@{((string)db["Server"]).Split(".").First()}";
    }

    /// <inheritdoc cref="ITelemetryProcessor.Process" />
    public void Process(ITelemetry item)
    {
        if (item is TraceTelemetry trace && trace.Properties.TryGetValue("CategoryName", out string categoryName) && categoryName == "Microsoft.EntityFrameworkCore.Database.Command")
        {
            trace.Properties.TryGetValue("commandText", out var commandText);
            trace.Properties.TryGetValue("elapsed", out var elapsed);
            int.TryParse(elapsed, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var duration);

            var queryMatch = Regex.Match(commandText, @"(SELECT|INSERT|UPDATE|DELETE)(.+\n?FROM| INTO)? (\w+)");
            var queryName = queryMatch.Groups.Count == 4 ? $"{queryMatch.Groups[1].Value} {queryMatch.Groups[3].Value}" : "query";

            var dependency = new DependencyTelemetry("SQL", _databaseName, queryName, commandText ?? string.Empty);

            dependency.Context.Cloud.RoleInstance = trace.Context.Cloud.RoleInstance;
            dependency.Context.Cloud.RoleName = trace.Context.Cloud.RoleName;
            dependency.Context.Location.Ip = trace.Context.Location.Ip;
            dependency.Context.InstrumentationKey = trace.Context.InstrumentationKey;
            dependency.Context.Operation.Id = trace.Context.Operation.Id;
            dependency.Context.Operation.Name = trace.Context.Operation.Name;
            dependency.Context.Operation.ParentId = trace.Context.Operation.ParentId;

            dependency.Duration = TimeSpan.FromMilliseconds(duration);
            dependency.Timestamp = trace.Timestamp - dependency.Duration;

            if (trace.SeverityLevel == SeverityLevel.Error)
            {
                dependency.Success = false;
                dependency.ResultCode = "Error";
            }

            item = dependency;
        }

        _next.Process(item);
    }
}
