namespace BetterRead.Bot.Dialogs

open System.Threading
open BetterRead.Bot.StateAccessors
open Microsoft.Bot.Builder.Dialogs

type MainDialog(accessors: BotStateAccessors) as this =
    inherit ComponentDialog()

    let initialStepAsync (stepContext: WaterfallStepContext) (cancellationToken: CancellationToken) =
        async {
            return! stepContext.BeginDialogAsync("MainDialog.greeting", null, cancellationToken) |> Async.AwaitTask
        } |> Async.StartAsTask
    
    let finalStepAsync (stepContext: WaterfallStepContext) (cancellationToken: CancellationToken) =
        async {
            return! stepContext.EndDialogAsync(null, cancellationToken) |> Async.AwaitTask
        } |> Async.StartAsTask
    
    do
        let waterfallSteps = [|
            WaterfallStep(initialStepAsync)
            WaterfallStep(finalStepAsync)
        |]
        
        this.AddDialog(GreetingDialog("MainDialog.greeting", accessors)) |> ignore
        this.AddDialog(WaterfallDialog("MainDialog.mainFlow", waterfallSteps)) |> ignore
        
        this.InitialDialogId <- "MainDialog.mainFlow"
