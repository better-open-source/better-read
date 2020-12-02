namespace BetterRead.Bot.Dialogs

open System

open Azure.Storage.Blobs
open Microsoft.Bot.Builder
open Microsoft.Bot.Schema
open Microsoft.Bot.Builder.Dialogs

open BetterRead.Bot.StateAccessors
open BetterRead.Application.Infra.GetBook
open BetterRead.Common.AsyncExtensions

module DownloadBookCommandModule =
    let (|DownloadBookCommand|_|) (msg:string) =
        if msg.Contains("/download:") then
            match Int32.TryParse(msg.Replace("/download:", "")) with
            | (true, id) -> Some id
            | _ -> None
        else
            None

module private InternalDownloadBookModule =
    
    [<Literal>]
    let mainFlowId = "DownloadBook.mainFlow"
    
    [<Literal>]
    let docxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    
    let getBlobBook (stepContext : WaterfallStepContext) (blobCli : BlobContainerClient) =
        stepContext.Options :?> Option<int>
           |> Option.map (fun id -> fetchBlobBook id blobCli)
           |> Async.traverseOpt
           |> Async.map (Option.bind id)
    
    let finalStepAsync (blobCli : BlobContainerClient) (stepContext : WaterfallStepContext) cancellationToken =
        async {
            match! getBlobBook stepContext blobCli with
            | Some blobBook ->
                let text = $"{blobBook.Name} available [here]({blobBook.Url.ToString()})!"
                let activity = MessageFactory.Text(text)
                do! stepContext.Context.SendActivityAsync(activity)
                    |> Async.AwaitTask |> Async.Ignore
                ()
            | None -> 
                do! stepContext.Context.SendActivityAsync("Document build failed")
                    |> Async.AwaitTask |> Async.Ignore
                ()
            
            return! stepContext.EndDialogAsync(null, cancellationToken) |> Async.AwaitTask
        } |> Async.StartAsTask

    let waterfallSteps (blobCli: BlobContainerClient) =
        [| WaterfallStep(finalStepAsync blobCli) |]
    
open InternalDownloadBookModule

type DownloadBookDialog
    ( dialogId    : string,
      _accessors  : BotStateAccessors,
      blobCli     : BlobContainerClient) as this =
    inherit ComponentDialog(dialogId)
    do
        this.AddDialog(WaterfallDialog(mainFlowId, waterfallSteps blobCli)) |> ignore
        this.InitialDialogId <- mainFlowId