namespace BetterRead.Bot.Dialogs

open System

open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Bot.Schema

open BetterRead.Bot.StateAccessors
open BetterRead.Common.AsyncExtensions
open BetterRead.Application.Domain.Book
open BetterRead.Application.Infra.BookInfoParser
open BetterRead.Application.Infra.HtmlWebFactory

module BookInfoCommandModule =
    let (|BookInfoCommand|_|) str =
        match Uri.TryCreate(str, UriKind.Absolute) with
        | (true, uri) -> getBookId uri
        | _ -> None
        
module private InternalBookInfoCommandModule =

    [<Literal>]
    let mainFlowId = "BookInfoDialog.mainFlow"
    
    let printInfoAsync (context:ITurnContext) (bookInfo:BookInfo option) =
        match bookInfo with
        | Some info -> async {
            let title = sprintf "Book: %s \n\n\u200CAuthor: %s" info.Name info.Author
            let buttons = [ CardAction (ActionTypes.ImBack, "Download", value = sprintf "/download:%d" info.Id) ]
            let card = HeroCard(title = title, buttons = ResizeArray<CardAction> buttons)
                
            match info.Image with
            | (Some _, imageUri) ->
                card.Images <- ResizeArray<CardImage> [ CardImage(imageUri.ToString(), imageUri.ToString()) ]
            | (None, _) -> () 
        
            let activity = MessageFactory.Attachment(card.ToAttachment())
            let! _= context.SendActivityAsync(activity) |> Async.AwaitTask
            () }
        | None -> async {
            let! _= context.SendActivityAsync("We are not able to read this book for some reason...") |> Async.AwaitTask
            () }
    
    let finalStepAsync (stepContext: WaterfallStepContext) cancellationToken =
        async {
            let idOpt = stepContext.Options :?> Option<int>
            let! infoOtp = idOpt |> Option.map (parseBookInfo htmlWeb)
                                 |> Async.traverseOpt
            do! printInfoAsync stepContext.Context infoOtp
            return! stepContext.EndDialogAsync(null, cancellationToken) |> Async.AwaitTask
        } |> Async.StartAsTask

    let waterfallSteps = [| WaterfallStep(finalStepAsync) |]
    
open InternalBookInfoCommandModule

type BookInfoDialog(dialogId:string, _accessors: BotStateAccessors) as this =
    inherit ComponentDialog(dialogId)
    do
        this.AddDialog(WaterfallDialog(mainFlowId, waterfallSteps)) |> ignore
        this.InitialDialogId <- mainFlowId