namespace BetterRead.Bot.Dialogs

open System.Threading
open BetterRead.Bot.StateAccessors
open Microsoft.Bot.Builder.Dialogs

module GreetingDialogModule =
    let (|GreetingCommand|_|) str =
        match str with
        | "/start" -> Some GreetingCommand
        | _ -> None

module private InternalGreetingDialogModule =
    
    [<Literal>]
    let greetingCommand = "/start" 
    
    [<Literal>]
    let mainFlowId = "GreetingDialog.mainFlow"
    
    let finalStepAsync (stepContext: WaterfallStepContext) (cancellationToken: CancellationToken) =
        async {
            let! _= stepContext.Context.SendActivityAsync("Hi from F#!") |> Async.AwaitTask
            return! stepContext.EndDialogAsync(null, cancellationToken) |> Async.AwaitTask
        } |> Async.StartAsTask

    let waterfallSteps = [|
        WaterfallStep(finalStepAsync)
    |]
    
open InternalGreetingDialogModule

type GreetingDialog(dialogId:string, accessors: BotStateAccessors) as this =
    inherit ComponentDialog(dialogId)
    do
        this.AddDialog(WaterfallDialog(mainFlowId, waterfallSteps)) |> ignore
        this.InitialDialogId <- mainFlowId
