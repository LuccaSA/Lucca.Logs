# Lucca.Logs

Gestion des logs pour les app et webservices lucca. La lib est adoss�e � Microsoft.Extensions.Logging et permet via un `ILogger<T>` d'envoyer les logs � la fois vers logmatic et OpServer.

Le contenu � destination de Logmatic passe par un logger NLog, et une structure de fichier sp�cifique � nos besoins sur logmatic.

Le contenu � destination de OpServer passer par un logger StackExchange.Exceptional.

## Utilisation 

En mode injection de d�pendance, il suffit de se faire injecter un `ILogger<T>` ou un `ILoggerFactory`, puis de logger de fa�on standard.

Plus d'infos ici : [Logging in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1&tabs=aspnetcore2x)


## Setup MVC6 / netcoreapp2 

Dans la m�thode `ConfigureServices` du startup.cs, ajoutez le registering suivant apr�s `services.AddMvc();`

```csharp
services.AddLogging(l =>
{
    l.AddLuccaLogs(Configuration.GetSection("LuccaLogs"));
});
services.AddSingleton<IExceptionQualifier, LuccaFacesExceptionQualifier>(); 
```

- `AddLuccaLogs` permet de register l'ensemble des classes n�cessaires � Lucca.Logs
- En param�tre, on prends la section du fichier de configuration n�c�ssaire. [Un exemple est dispo ici](https://github.com/LuccaSA/Lucca.Logs/blob/master/Lucca.Logs.Tests/Configs/standard.json).
- Vous pouvez utiliser une impl�mentation perso d'un `IExceptionQualifier` permettant de piloter le comportement du logger selon les exceptions

Dans la m�thode `Configure`, ajoutez ceci avant `app.UseMvc();` (Attention � l'ordre)

```csharp
if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseLuccaLogs(new LuccaExceptionHandlerOption()
    {
        HtmlResponse = ExceptionMessage.GenerateHtmlError
    });
}
```

- `app.UseDeveloperExceptionPage();` permet de b�n�ficier en dev local d'informations d�taill�es sur les exceptions lorsqu'elles se produisent
- `app.UseLuccaLogs();` permet d'enregister le middleware permettant d'intercepter les exceptions, et de produire une erreur de sortie correspondante aux settings
    - `LuccaExceptionHandlerOption` permet de customiser la generation du message d'erreur
    - 3 handlers pour textplain / html et json sont disponibles

## Setup MVC5 / net461

En MVC5, la librairie utilise actuellement en interne la DI de `Microsoft.Extensions.DependencyInjection`. Il sera possible � terme de brancher n'importe quel DI framework (mais pas pour le moment)

Dans le Global.asax.cs, ajouter le register suivant : 

```csharp
GlobalConfiguration.Configuration.Services.AddLuccaLogs();
Logger.DefaultFactory = LoggerBuilder.CreateLuccaLogsFactory(builder =>
{
    builder.Services.AddSingleton<IExceptionQualifier, LuccaExceptionQualifier>();
    builder.AddLuccaLogs(luccaLogsOptions =>
    {
        luccaLogsOptions.ConnectionString = connectionString;
        luccaLogsOptions.LogFilePath = logFile;
    });
});
```

- `Services.AddLuccaLogs()` permet de register la lib sur les hooks de MVC5
- `Logger.DefaultFactory = ...` permet de customiser les types registered, et d'ajouter vos propres impl�mentations

## Utilisation

Faites vous injecter un `ILogger<T>` dans le constructeur des classes o� vous souhaitez logger.
Utilisez les m�thodes standard du framework pour logger ( `_logger.LogError(...)` etc)

Lisez la doc : https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1


## Unit tests

Le moyen le plus simple pour mocker un `ILogger<T>` est d'instancier un `NullLogger<T>`.
