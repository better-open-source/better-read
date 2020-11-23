namespace BetterRead.Bot.Dialogs

open System.Threading
open BetterRead.Bot.StateAccessors
open Microsoft.Bot.Builder.Dialogs

module MainDialogModule =

    [<Literal>]
    let mainFlowId = "MainDialog.mainFlow"

    [<Literal>]
    let greetingId = "MainDialog.greeting"

    let initialStepAsync (stepContext: WaterfallStepContext) (cancellationToken: CancellationToken) =
        async {
            match stepContext.Context.Activity.Text with
            | _ -> failwith ""
            return! stepContext.BeginDialogAsync(greetingId, null, cancellationToken) |> Async.AwaitTask
        } |> Async.StartAsTask
    
    let finalStepAsync (stepContext: WaterfallStepContext) (cancellationToken: CancellationToken) =
        async {
            return! stepContext.EndDialogAsync(null, cancellationToken) |> Async.AwaitTask
        } |> Async.StartAsTask

    let waterfallSteps = [|
        WaterfallStep(initialStepAsync)
        WaterfallStep(finalStepAsync)
    |]
    
open MainDialogModule

type MainDialog(accessors: BotStateAccessors) as this =
    inherit ComponentDialog()
    do
        this.AddDialog(GreetingDialog(greetingId, accessors)) |> ignore
        this.AddDialog(WaterfallDialog(mainFlowId, waterfallSteps)) |> ignore
        this.InitialDialogId <- mainFlowId
