namespace BetterRead.Bot.Dialogs

open System.Threading
open BetterRead.Bot.StateAccessors
open Microsoft.Bot.Builder.Dialogs

type GreetingDialog(dialogId:string, accessors: BotStateAccessors) as this =
    inherit ComponentDialog(dialogId)

    let finalStepAsync (stepContext: WaterfallStepContext) (cancellationToken: CancellationToken) =
        async {
            let! _= stepContext.Context.SendActivityAsync("Hi from F#!") |> Async.AwaitTask
            return! stepContext.EndDialogAsync(null, cancellationToken) |> Async.AwaitTask
        } |> Async.StartAsTask
    
    do
        let waterfallSteps = [|
            WaterfallStep(finalStepAsync)
        |]
        
        this.AddDialog(WaterfallDialog("GreetingDialog.mainFlow", waterfallSteps)) |> ignore
        
        this.InitialDialogId <- "GreetingDialog.mainFlow"
