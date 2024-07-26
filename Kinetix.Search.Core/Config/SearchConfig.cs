using System.Text.RegularExpressions;

namespace Kinetix.Search.Core.Config;

/// <summary>
/// Section de configuration du moteur de recherche.
/// </summary>
public class SearchConfig
{
    public Dictionary<string, SearchConfigItem> Servers { get; set; } = [];

    public int ClusterSize { get; set; } = 5000;

    public string GetIndexNameForType(string dataSourceName, Type documentType)
    {
        var connSettings = GetServer(dataSourceName);
        return $@"{connSettings.IndexName}_{GetTypeNameForIndex(documentType)}";
    }

    public SearchConfigItem GetServer(string dataSourceName)
    {
        if (!Servers.TryGetValue(dataSourceName, out var server))
        {
            throw new ArgumentException($@"Le server de recherche ""{dataSourceName}"" est introuvable dans la configuration.");
        }

        return server;
    }

    /// <summary>
    /// Récupère le nom du document pour déterminer le nom de l'index.
    /// </summary>
    /// <param name="documentType">Type du document.</param>
    /// <returns>Nom.</returns>
    public static string GetTypeNameForIndex(Type documentType)
    {
        return Regex.Replace(Regex.Replace(documentType.Name, "Document$", string.Empty), @"\p{Lu}", m => "_" + m.Value)
            .Substring(1).ToLowerInvariant();
    }
}
