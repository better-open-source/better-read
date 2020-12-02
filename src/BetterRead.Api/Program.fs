namespace BetterRead.Api

open System
open System.Text
open Azure.Storage.Blobs
open BetterRead.Bot.Bots
open BetterRead.Bot.StateAccessors
open BetterRead.Bot.Dialogs
open BetterRead.Bot.AdapterWithErrorHandler
open Giraffe
open Giraffe.EndpointRouting
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Bot.Builder.Integration
open Microsoft.Bot.Builder.Integration.AspNet.Core
open Microsoft.Bot.Connector.Authentication
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

module private BetterReadApiModule =

    let botHandler : HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let bot = ctx.GetService<IBot>()
                let adapter = ctx.GetService<IBotFrameworkHttpAdapter>()
                do! adapter.ProcessAsync(ctx.Request, ctx.Response, bot) |> Async.AwaitTask
                return! Successful.OK None next ctx
            }

    let errorHandler (ex : Exception) (logger : ILogger) =
        logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
        clearResponse >=> setStatusCode 500 >=> text ex.Message

    let endpoints = [
        GET => route "/"     (text "Bot is running")
        GET => route "/ping" (text "pong")
        subRoute "/api" [
            route "/messages" botHandler
        ]
    ]

    let configureApp (app : IApplicationBuilder) =
        app.UseGiraffeErrorHandler(errorHandler)
           .UseDefaultFiles()
           .UseStaticFiles()
           .UseWebSockets()
           .UseRouting()
           .UseBotFramework()
           .UseResponseCaching()
           .UseEndpoints(fun e -> e.MapGiraffeEndpoints(endpoints))
           |> ignore

    let configureServices (services : IServiceCollection) =
        let configuration = services.BuildServiceProvider().GetService<IConfiguration>()
        services
            .AddBot<DialogBot<MainDialog>>(
                fun (options : BotFrameworkOptions) ->
                    let appId = configuration.["microsoftAppId"]
                    let appPass = configuration.["microsoftAppPassword"]
                    options.CredentialProvider <- SimpleCredentialProvider(appId, appPass)
                    ())
            .AddBotFrameworkAdapterIntegration()
            .AddTransient<Dialog, MainDialog>()
            .AddTransient<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>()
            .AddTransient<ConversationState>()
            .AddTransient<BotStateAccessors>()
            .AddTransient<IStorage, MemoryStorage>()
            .AddTransient<BlobContainerClient>(
                fun sp ->
                    let connectionString = configuration.GetConnectionString "blobConnectionString"
                    let blobService = BlobServiceClient(connectionString)
                    blobService.GetBlobContainerClient configuration.["blobContainerName"])
            .AddResponseCaching()
            .AddGiraffe() |> ignore

    let configureLogging (loggerBuilder : ILoggingBuilder) =
        loggerBuilder.AddFilter(fun lvl -> lvl.Equals LogLevel.Error)
                     .AddConsole()
                     .AddDebug() |> ignore

    let result = 0
    
    [<EntryPoint>]
    let main _ =
        Encoding.RegisterProvider CodePagesEncodingProvider.Instance
        Console.OutputEncoding <- Encoding.UTF8
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(
                fun webHostBuilder ->
                    webHostBuilder
                        .Configure(configureApp)
                        .ConfigureServices(configureServices)
                        .ConfigureLogging(configureLogging)
                    |> ignore)
            .Build()
            .Run()
        result