namespace BetterRead.Bot.Dialogs

open System

open Microsoft.Bot.Builder
open Microsoft.Bot.Schema
open Microsoft.Bot.Builder.Dialogs

open BetterRead.Bot.Domain.Book
open BetterRead.Bot.StateAccessors
open BetterRead.Bot.Infra.BookInfoParser
open BetterRead.Bot.Infra.BookSheetsParser
open BetterRead.Bot.Infra.BookBuilder
open BetterRead.Bot.Infra.HtmlWebFactory

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
    
    let finalStepAsync (stepContext: WaterfallStepContext) cancellationToken =
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

    let waterfallSteps = [| WaterfallStep(finalStepAsync) |]
    
open InternalDownloadBookModule

type DownloadBookDialog(dialogId:string, _accessors: BotStateAccessors) as this =
    inherit ComponentDialog(dialogId)
    do
        this.AddDialog(WaterfallDialog(mainFlowId, waterfallSteps)) |> ignore
        this.InitialDialogId <- mainFlowId