namespace BetterRead.Bot.Dialogs

open Azure.Storage.Blobs
open Microsoft.Bot.Builder.Dialogs

open BetterRead.Bot.StateAccessors
open BetterRead.Bot.Dialogs.GreetingDialogModule
open BetterRead.Bot.Dialogs.BookInfoCommandModule
open BetterRead.Bot.Dialogs.DownloadBookCommandModule

module private InternalMainDialogModule =

    [<Literal>]
    let mainFlowId = "MainDialog.mainFlow"

    [<Literal>]
    let greetingId = "MainDialog.greeting"
    
    [<Literal>]
    let bookInfoId = "MainDialog.bookInfo"
    
    [<Literal>]
    let downloadBookId = "MainDialog.downloadBook"
    
    let beginDialogAsync (stepContext: WaterfallStepContext) cancellationToken dialogId options =
        async {
            return! stepContext.BeginDialogAsync(dialogId, options, cancellationToken) |> Async.AwaitTask
        }
    
    let initialStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        async {
            let beginAsync = beginDialogAsync stepContext cancellationToken
            match stepContext.Context.Activity.Text with
            | GreetingCommand        -> return! beginAsync greetingId      None
            | BookInfoCommand     id -> return! beginAsync bookInfoId     (Some id)
            | DownloadBookCommand id -> return! beginAsync downloadBookId (Some id)
            | _                      -> return  failwith "Invalid command!"
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

type MainDialog(accessors: BotStateAccessors, blobCli: BlobContainerClient) as this =
    inherit ComponentDialog()
    do
        this.AddDialog(GreetingDialog     (greetingId, accessors))              |> ignore
        this.AddDialog(BookInfoDialog     (bookInfoId, accessors))              |> ignore
        this.AddDialog(DownloadBookDialog (downloadBookId, accessors, blobCli)) |> ignore
        this.AddDialog(WaterfallDialog    (mainFlowId, waterfallSteps))         |> ignore
        this.InitialDialogId <- mainFlowId
