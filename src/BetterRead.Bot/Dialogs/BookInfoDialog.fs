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
    
    let sendCardActivity (context:ITurnContext) (card:HeroCard) =
        async {
            let activity = MessageFactory.Attachment(card.ToAttachment())
            let! _= context.SendActivityAsync(activity) |> Async.AwaitTask
            ()
        } |> Async.Ignore
    
    let printInfo (context:ITurnContext) (bookInfo:BookInfo option) =
        match bookInfo with
        | Some info ->
            let title = sprintf "Book: %s \n\n\u200CAuthor: %s" info.Name info.Author
            let downloadCommand = sprintf "download:%s" (info.Url.ToString())
            let buttons = ResizeArray<CardAction> [
                CardAction (ActionTypes.ImBack, "Download", value = downloadCommand) ]
            let card = HeroCard(title = title, buttons = buttons)
                
            match info.Image with
            | (Some _, imageUri) -> async {
                card.Images <- ResizeArray<CardImage> [CardImage(imageUri.ToString(), imageUri.ToString())]
                do! sendCardActivity context card
                ignore 0 }
            | (None, _) -> async {
                do! sendCardActivity context card
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

    let waterfallSteps = [| WaterfallStep(finalStepAsync) |]
    
open InternalBookInfoCommandModule

type BookInfoDialog(dialogId:string, accessors: BotStateAccessors) as this =
    inherit ComponentDialog(dialogId)
    do
        this.AddDialog(WaterfallDialog(mainFlowId, waterfallSteps)) |> ignore
        this.InitialDialogId <- mainFlowId