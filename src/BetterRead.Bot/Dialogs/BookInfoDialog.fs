namespace BetterRead.Bot.Dialogs

open System

open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Bot.Schema

open BetterRead.Bot.Configuration.AsyncExtensions
open BetterRead.Bot.Domain.Book
open BetterRead.Bot.StateAccessors
open BetterRead.Bot.Infra.BookInfoParser
open BetterRead.Bot.Infra.HtmlWebFactory

module BookInfoCommandModule =
    let (|BookInfoCommand|_|) str =
        match Uri.TryCreate(str, UriKind.Absolute) with
        | (true, uri) -> getBookId uri
        | _ -> None
        
module InternalBookInfoCommandModule =

    [<Literal>]
    let mainFlowId = "BookInfoDialog.mainFlow"
    
    let printInfo (context:ITurnContext) (bookInfo:BookInfo option) =
        match bookInfo with
        | Some info -> 
            let msg = sprintf "Book: %s \n\n\u200CAuthor: %s" info.Name info.Author
            match info.Image with
            | (Some content, imageUri) -> async {
                let imgContent = Convert.ToBase64String content
                let attachment = Attachment ("image/png", sprintf "data:image/png;base64,%s" imgContent, info.Name, imageUri.ToString())
                let activity = MessageFactory.Attachment(attachment, msg)
                let! _= context.SendActivityAsync(activity) |> Async.AwaitTask
                ignore 0 }
            | (None, _) -> async {
                let! _= context.SendActivityAsync(msg) |> Async.AwaitTask
                ignore 0 }
        | None -> async {
            let! _= context.SendActivityAsync("We are not able to read this book for some reason...") |> Async.AwaitTask
            ignore 0 }
    
    let finalStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        async {
            let! infoOtp = stepContext.Options :?> Option<int>
                          |> Option.map (parseBookInfo htmlWeb)
                          |> Async.traverseOpt
            
            do! printInfo stepContext.Context infoOtp
            
            return! stepContext.EndDialogAsync(null, cancellationToken) |> Async.AwaitTask
        } |> Async.StartAsTask

    let waterfallSteps = [|
        WaterfallStep(finalStepAsync)
    |]
    
open InternalBookInfoCommandModule

type BookInfoDialog(dialogId:string, accessors: BotStateAccessors) as this =
    inherit ComponentDialog(dialogId)
    do
        this.AddDialog(WaterfallDialog(mainFlowId, waterfallSteps)) |> ignore
        this.InitialDialogId <- mainFlowId