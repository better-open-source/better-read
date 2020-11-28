namespace BetterRead.Api

open BetterRead.Bot.Bots
open BetterRead.Bot.AdapterWithErrorHandler
open BetterRead.Bot.Dialogs
open BetterRead.Bot.StateAccessors

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Bot.Builder.Integration
open Microsoft.Bot.Builder.Integration.AspNet.Core
open Microsoft.Bot.Connector.Authentication
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Azure.Storage.Blobs

type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    member this.ConfigureServices(services: IServiceCollection) =
        services
            .AddBot<DialogBot<MainDialog>>(fun (options:BotFrameworkOptions) ->
                let appId = this.Configuration.["microsoftAppId"]
                let appPass = this.Configuration.["microsoftAppPassword"]
                options.CredentialProvider <- SimpleCredentialProvider(appId, appPass)
                ())
            .AddBotFrameworkAdapterIntegration()
            .AddTransient<Dialog, MainDialog>()
            .AddTransient<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>()
            .AddTransient<ConversationState>()
            .AddTransient<BotStateAccessors>()
            .AddTransient<IStorage, MemoryStorage>()
            .AddTransient<BlobContainerClient>(fun sp ->
                let connectionString = this.Configuration.GetConnectionString "blobConnectionString"
                let blobService = BlobServiceClient(connectionString)
                blobService.GetBlobContainerClient this.Configuration.["blobContainerName"])
            .AddControllers()
            .AddNewtonsoftJson()
        |> ignore

    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore

        app.UseDefaultFiles()
            .UseStaticFiles()
            .UseWebSockets()
            .UseRouting()
            .UseAuthorization()
            .UseBotFramework()
            .UseEndpoints(fun endpoints ->
                endpoints.MapControllers() |> ignore
                endpoints.MapDefaultControllerRoute() |> ignore
            ) |> ignore

    member val Configuration : IConfiguration = null with get, set
