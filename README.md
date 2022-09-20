# Lucca.Logs

Gestion des logs pour les app et webservices lucca. La lib est adossée à Microsoft.Extensions.Logging et permet via un `ILogger<T>` d'envoyer les logs à la fois vers un fichier (ingéré par Datadog) et OpServer.

Le contenu à destination de Datadog passe par un logger serilog qui écrit dans un fichier en json.

Le contenu à destination de OpServer passer par un logger StackExchange.Exceptional.

## Utilisation

En mode injection de dépendance, il suffit de se faire injecter un `ILogger<T>` ou un `ILoggerFactory`, puis de logger de façon standard.

Plus d'infos ici : [Logging in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1&tabs=aspnetcore2x)

## Setup net6.0

Dans votre Program.cs, initiliser le logger de la manière suivante : 

```csharp
LuccaLoggerAspnetCoreExtensions.InitLuccaLogs();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLuccaLogs();
builder.Services.AddLuccaLogs(builder.Configuration.GetSection("LuccaLogs"), "MyApp");
builder.Services.AddSingleton<IExceptionQualifier, GenericExceptionQualifier>(); 

 
...
var app = builder.Build();
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseLuccaLogs(new LuccaExceptionHandlerOption());
}

```

- `builder.host.UseLuccaLogs()` init le logger serilog.
- `builder.Services.AddLuccaLogs` permet de register l'ensemble des classes nécessaires à Lucca.Logs
- En paramètre, on prends la section du fichier de configuration nécessaire.
- Vous pouvez utiliser une implémentation perso d'un `IExceptionQualifier` au lieu de `GenericExceptionQualifier` permettant de piloter le comportement du logger selon les exceptions

- `app.UseDeveloperExceptionPage();` permet de bénéficier en dev local d'informations détaillées sur les exceptions lorsqu'elles se produisent
- `app.UseLuccaLogs();` permet d'enregister le middleware permettant d'intercepter les exceptions, et de produire une erreur de sortie correspondante aux settings
  - `LuccaExceptionHandlerOption` permet de customiser la generation du message d'erreur
  - 3 handlers pour textplain / html et json sont disponibles


## Setup MVC5 / net461

En MVC5, la librairie utilise actuellement en interne la DI de `Microsoft.Extensions.DependencyInjection`. Il sera possible à terme de brancher n'importe quel DI framework (mais pas pour le moment)

Dans le Global.asax.cs, ajouter le register suivant :

```csharp
lobalConfiguration.Configuration.Services.AddLuccaLogs();
GlobalConfiguration.Configuration.Services.Replace(typeof(System.Web.Http.ExceptionHandling.IExceptionHandler), new CCExceptionHandler());

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<IExceptionQualifier, ExceptionQualifier>();
serviceCollection.AddLuccaLogs(luccaLogsOptions =>
{
    luccaLogsOptions.ConnectionString = ConfigModelBase.Current.LuccaLogs.ConnectionString;
    luccaLogsOptions.LogFilePath = ConfigModelBase.Current.LuccaLogs.LogFilePath;
}, "CC Master");
Serilog.Log.Logger = serviceCollection.BuildServiceProvider().GetRequiredService<Serilog.ILogger>();
```

- `Services.AddLuccaLogs()` permet de register la lib sur les hooks de MVC5
- `ServiceCollection.AddLuccaLogs(...` permet d'initialiser le logger

## Utilisation net461

Faites vous injecter un `ILogger<T>` dans le constructeur des classes où vous souhaitez logger.
Utilisez les méthodes standard du framework pour logger ( `_logger.LogError(...)` etc)

Lisez la doc [à cette adresse](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1)

## Unit tests

Le moyen le plus simple pour mocker un `ILogger<T>` est d'instancier un `NullLogger<T>`.

## Envoyer des logs sur OpServer en local

Il est possible d'envoyer les erreurs sur OpServer pour les voir [à cette addresse](http://opserver.lucca.local/exceptions?store=Dev).

Pour celà, il faut modifier le fichier `appSettings.json` du projet concerné avec :

```json
 {
    "LuccaLoggerOptions": {
        "ApplicationName": "MY_BEAUTIFUL_APP Local",
        "ConnectionString": "Data Source=exceptions.lucca.local;Initial Catalog=Dev.Exceptions;User Id=opdev;Password=###############;",
        "LogFilePath": "C:\\Sites\\_logs\\lucca.MY_BEAUTIFUL_APP.logs",
        "IgnoreEmptyEventId": "false"
     }
}
```

> Demandez le mot de passe à l'équipe plateforme.

## Enrichir vos logs sur OpServer

Compléter le dictionnaire `Data` de vos `Exception` en préfixant les clés avec `Lucca.`

Vous retrouverez vos données custom dans la section `Custom` d'une exception sur OpServer.
