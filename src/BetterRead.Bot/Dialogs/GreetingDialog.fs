namespace BetterRead.Bot.Dialogs

open System.Reflection

open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Bot.Schema

open BetterRead.Common.AsyncExtensions
open BetterRead.Bot.StateAccessors

module GreetingDialogModule =
    
    let (|GreetingCommand|_|) str =
        match str with
        | "/start" -> Some GreetingCommand
        | _ -> None

module private InternalGreetingDialogModule =
    
    [<Literal>]
    let mainFlowId = "GreetingDialog.mainFlow"
    
    [<Literal>]
    let cardImage = "https://github.com/better-open-source/better-read/raw/main/assets/icon.png"
    
    let botVersion =
        Assembly.GetExecutingAssembly().GetName().Version.ToString(3)
    
    let finalStepAsync (stepContext : WaterfallStepContext) cancellationToken =
        async {
            let title = $"BetterRead bot (v.{botVersion})"
            let text = "Greetings! Gonna read books offline? LoveRead books parser and document generator tool is ready to help!"
            let image = ResizeArray<CardImage> [ CardImage(cardImage) ]
            let card = HeroCard(title = title, text = text, images = image)
            let activity = MessageFactory.Attachment(card.ToAttachment())
            
            do! stepContext.Context.SendActivityAsync(activity) |> Async.AwaitTask |> Async.Ignore
            return! stepContext.EndDialogAsync(null, cancellationToken) |> Async.AwaitTask
        } |> Async.StartAsTask

    let waterfallSteps = [| WaterfallStep(finalStepAsync) |]
    
open InternalGreetingDialogModule

type GreetingDialog
    ( dialogId    : string,
      _accessors  : BotStateAccessors) as this =
    inherit ComponentDialog(dialogId)
    do
        this.AddDialog(WaterfallDialog(mainFlowId, waterfallSteps)) |> ignore
        this.InitialDialogId <- mainFlowId
