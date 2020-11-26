namespace BetterRead.Bot

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

type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    member this.ConfigureServices(services: IServiceCollection) =
        services
            .AddBot<DialogBot<MainDialog>>(fun (options:BotFrameworkOptions) ->
                let appId = this.Configuration.["MicrosoftAppId"]
                let appPass = this.Configuration.["MicrosoftAppPassword"]
                options.CredentialProvider <- SimpleCredentialProvider(appId, appPass)
                ignore 0)
            .AddBotFrameworkAdapterIntegration()
            .AddTransient<Dialog, MainDialog>()
            .AddTransient<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>()
            .AddTransient<ConversationState>()
            .AddTransient<BotStateAccessors>()
            .AddTransient<IStorage, MemoryStorage>()
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
