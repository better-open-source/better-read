namespace BetterRead.Bot

open BetterRead.Bot.Bots
open BetterRead.Bot.AdapterWithErrorHandler
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Integration.AspNet.Core
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    member this.ConfigureServices(services: IServiceCollection) =
        services
            .AddSingleton<IBot, EchoBot>()
            .AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>()
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
            .UseEndpoints(fun endpoints ->
                endpoints.MapControllers() |> ignore
            ) |> ignore

    member val Configuration : IConfiguration = null with get, set
