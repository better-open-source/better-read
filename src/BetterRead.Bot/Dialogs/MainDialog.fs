namespace BetterRead.Bot.Dialogs

open System.Threading
open Microsoft.Bot.Builder.Dialogs

open BetterRead.Bot.Dialogs.GreetingDialogModule
open BetterRead.Bot.Dialogs.BookInfoCommandModule
open BetterRead.Bot.StateAccessors

module private InternalMainDialogModule =

    [<Literal>]
    let mainFlowId = "MainDialog.mainFlow"

    [<Literal>]
    let greetingId = "MainDialog.greeting"
    
    [<Literal>]
    let bookInfoId = "MainDialog.bookInfo"
    
    let beginDialogAsync (stepContext: WaterfallStepContext) cancellationToken dialogId options =
        async {
            return! stepContext.BeginDialogAsync(dialogId, options, cancellationToken) |> Async.AwaitTask
        }
    
    let initialStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        async {
            let beginAsync = beginDialogAsync stepContext cancellationToken
            match stepContext.Context.Activity.Text with
            | GreetingCommand    -> return! beginAsync greetingId null
            | BookInfoCommand id -> return! beginAsync bookInfoId (id :> obj)
            | _                  -> return failwith "Invalid command!"
        } |> Async.StartAsTask
    
    let finalStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        async {
            return! stepContext.EndDialogAsync(null, cancellationToken) |> Async.AwaitTask
        } |> Async.StartAsTask

    let waterfallSteps = [|
        WaterfallStep(initialStepAsync)
        WaterfallStep(finalStepAsync)
    |]
    
open InternalMainDialogModule

type MainDialog(accessors: BotStateAccessors) as this =
    inherit ComponentDialog()
    do
        this.AddDialog(GreetingDialog(greetingId, accessors)) |> ignore
        this.AddDialog(WaterfallDialog(mainFlowId, waterfallSteps)) |> ignore
        this.InitialDialogId <- mainFlowId
