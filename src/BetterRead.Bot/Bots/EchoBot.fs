namespace BetterRead.Bot.Bots

open System.Threading
open System.Threading.Tasks
open Microsoft.Bot.Builder
open Microsoft.Bot.Schema
open Microsoft.Extensions.Logging

type EchoBot(logger : ILogger<EchoBot>) =
    inherit ActivityHandler()
    override x.OnTurnAsync(turnCtx: ITurnContext, cancellationToken: CancellationToken) =
        async {
            match turnCtx.Activity.Type with
            | ActivityTypes.Message ->
                turnCtx.SendActivityAsync("Hi from F#!!") |> Async.AwaitTask |> ignore
                let responseMessage = sprintf "You sent '%s'\n" turnCtx.Activity.Text
                logger.LogInformation responseMessage
                turnCtx.SendActivityAsync(responseMessage) |> Async.AwaitTask |> ignore
            | _ ->
                turnCtx.SendActivityAsync(sprintf "{%s} event detected" turnCtx.Activity.Type)
                |> Async.AwaitTask |> ignore     
        } |> Async.StartAsTask :> Task