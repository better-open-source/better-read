namespace BetterRead.Bot.AdapterWithErrorHandler

open System
open System.Threading.Tasks

open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Integration.AspNet.Core
open Microsoft.Bot.Builder.TraceExtensions;
open Microsoft.Extensions.Logging

type AdapterWithErrorHandler () =
    inherit BotFrameworkHttpAdapter()
    new (logger : ILogger<BotFrameworkHttpAdapter>) as this =
        AdapterWithErrorHandler() then
            
        let onTurnError (ctx:ITurnContext) (ex:Exception) =
            async {
                logger.LogError(sprintf "[OnTurnError] unhandled error : %s" ex.Message)
                let! _ = ctx.SendActivityAsync "The bot encountered an error or bug." |> Async.AwaitTask
                let! _ = ctx.SendActivityAsync "To continue to run this bot, please fix the bot source code." |> Async.AwaitTask
                let! _ = ctx.TraceActivityAsync ("OnTurnError Trace", ex.Message, "https://www.botframework.com/schemas/error", "TurnError") |> Async.AwaitTask
                ignore 0
            } |> Async.StartAsTask :> Task

        this.OnTurnError <- Func<_,_,_>(onTurnError)