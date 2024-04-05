# Kinetix (pour .NET 8)

Kinetix est un ensemble de modules destiné à enrichir une application .NET de fonctionnalités transverses pour simplifier certaines opérations courantes. Ces modules sont relativement indépendants et peuvent donc être intégrés unitairement.

Ci-dessous, la liste des fonctionnalités disponibles par module :

## `Kinetix.Services`

Divisés en deux modules :

- `Kinetix.Services` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.Services)](https://www.nuget.org/packages/Kinetix.Services)
- `Kinetix.Services.Annotations` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.Services.Annotations)](https://www.nuget.org/packages/Kinetix.Services.Annotations)

Fonctionnalités :

- Enregistrement automatique des services via les annotations `[RegisterContract]`, `[RegisterImpl]` et la méthode d'extension `AddServices()`
- Intégration d'intercepteurs (optionnels) sur les services pour y intégrer des fonctionnalités transverses (transactions, logs...)
- Gestion transverse de "contextes de transaction" pour rattacher de l'état à une transaction courante et implémenter des actions au commit.
- Manager de listes de références avec cache
- Manager de services de téléchargement de fichiers

## `Kinetix.Modeling` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.Modeling)](https://www.nuget.org/packages/Kinetix.Modeling)

- Gestion de domaines métier (avec validation) sur des champs de classes

## `Kinetix.Search`

Divisé en trois modules :

- `Kinetix.Search.Core` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.Search.Core)](https://www.nuget.org/packages/Kinetix.Search.Core)
- `Kinetix.Search.Models` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.Search.Models)](https://www.nuget.org/packages/Kinetix.Search.Models)
- `Kinetix.Search.Elastic` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.Search.Elastic)](https://www.nuget.org/packages/Kinetix.Search.Elastic)

Fonctionnalités :

- API de recherche avancée à facettes
- Gestion de l'alimentation (transactionnelle) de l'index de recherche (implémentée avec `Kinetix.Services`)
- Implémentation de l'API avec ElasticSearch 7

## `Kinetix.Monitoring`

Divisé en deux modules :

- `Kinetix.Monitoring.Core` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.Monitoring.Core)](https://www.nuget.org/packages/Kinetix.Monitoring.Core)
- `Kinetix.Monitoring.Insights` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.Monitoring.Insights)](https://www.nuget.org/packages/Kinetix.Monitoring.Insights)

Fonctionnalités :

- Gestion de logs de services (implémentés comme intercepteur pour `Kinetix.Services`)
- Publication des logs dans Azure ApplicationInsights

## `Kinetix.Reporting`

Divisé en trois modules :

- `Kinetix.Reporting.Annotations` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.Reporting.Annotations)](https://www.nuget.org/packages/Kinetix.Reporting.Annotations)
- `Kinetix.Reporting.Core` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.Reporting.Core)](https://www.nuget.org/packages/Kinetix.Reporting.Core)
- `Kinetix.Reporting.Web` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.Reporting.Web)](https://www.nuget.org/packages/Kinetix.Reporting.Web)

Fonctionnalités :

- Génération d'exports Excels à partir de modèles Kinetix (annotés avec `Kinetix.Modeling` et qui utilisent des listes de référence de `Kinetix.Services`).

## `Kinetix.User` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.User)](https://www.nuget.org/packages/Kinetix.User)

- Abstraction pour accéder à l'utilisateur connecté (pour ne pas toujours devoir utilisé celui de `HttpContext.User`)

## `Kinetix.Web` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.Web)](https://www.nuget.org/packages/Kinetix.Web)

- Divers filtres MVC génériques
- Préconfiguration de la sérialisation JSON

## `Kinetix.EFCore` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.EFCore)](https://www.nuget.org/packages/Kinetix.EFCore)

- Intégration d'EF Core dans le système de transaction de `Kinetix.Services`

## `Kinetix.DataAccess.Sql`

Divisé en trois modules :

- `Kinetix.DataAccess.Sql` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.DataAccess.Sql)](https://www.nuget.org/packages/Kinetix.DataAccess.Sql)
- `Kinetix.DataAccess.Sql.SqlServer` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.DataAccess.Sql.SqlServer)](https://www.nuget.org/packages/Kinetix.DataAccess.Sql.SqlServer)
- `Kinetix.DataAccess.Sql.Postgres` [![NuGet Badge](https://badgen.net/nuget/v/Kinetix.DataAccess.Sql.Postgres)](https://www.nuget.org/packages/Kinetix.DataAccess.Sql.Postgres)

Il s'agit d'un ORM "legacy" (utilisez plutôt [EF Core](https://docs.microsoft.com/en-us/ef/core/) et/ou [Dapper](https://dapper-tutorial.net/dapper)), avec une implémentation pour SQL Server et PostgreSQL.

Fonctionnalités :

- Requêtes SQL dynamiques (via une syntaxe spéciale)
- "Broker" pour gérer du CRUD simple sur des modèles persistés.
