namespace BetterRead.Bot.Dialogs

open System

open Azure.Storage.Blobs
open Microsoft.Bot.Builder
open Microsoft.Bot.Schema
open Microsoft.Bot.Builder.Dialogs

open BetterRead.Bot.StateAccessors
open BetterRead.Application.Domain.Book
open BetterRead.Application.Parsers.BookInfoParser
open BetterRead.Application.Parsers.BookSheetsParser
open BetterRead.Application.Parsers.BookBuilder
open BetterRead.Application.Infra.HtmlWebFactory

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
    
    let sendDocument (stepContext: ITurnContext) book = async {
        let bytes = generateDocument book
        let fileName = sprintf "%s.docx" book.Info.Name
        let attachment = Attachment(docxContentType, content = bytes, name = fileName)
        let activity = MessageFactory.Attachment(attachment)
        let! _= stepContext.SendActivityAsync(activity) |> Async.AwaitTask
        () }
    
    let finalStepAsync (blobCli: BlobContainerClient) (stepContext: WaterfallStepContext) cancellationToken =
        async {
            match stepContext.Options :?> Option<int> with
            | Some id ->
                let! info = parseBookInfo htmlWeb id
                let! sheets = parseSheets htmlWeb id
                do! sendDocument stepContext.Context {Info = info; Sheets = sheets}
                ()
            | None -> 
                let! _= stepContext.Context.SendActivityAsync("Document build failed") |> Async.AwaitTask
                ()
            
            return! stepContext.EndDialogAsync(null, cancellationToken) |> Async.AwaitTask
        } |> Async.StartAsTask

    let waterfallSteps (blobCli: BlobContainerClient) =
        [| WaterfallStep(finalStepAsync blobCli) |]
    
open InternalDownloadBookModule

type DownloadBookDialog(dialogId: string, _accessors: BotStateAccessors, blobCli: BlobContainerClient) as this =
    inherit ComponentDialog(dialogId)
    do
        this.AddDialog(WaterfallDialog(mainFlowId, waterfallSteps blobCli)) |> ignore
        this.InitialDialogId <- mainFlowId