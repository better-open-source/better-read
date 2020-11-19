namespace BetterRead.Bot.Bots

open System.Threading
open System.Threading.Tasks
open BetterRead.Bot.StateAccessors
open Microsoft.Bot.Builder
open Microsoft.Bot.Schema
open Microsoft.Bot.Builder.Dialogs

type DialogBot<'TDialog when 'TDialog :> Dialog>
    ( dialog : Dialog,
      accessors: BotStateAccessors) =
    inherit ActivityHandler()
    
    override x.OnMessageActivityAsync
        ( turnContext: ITurnContext<IMessageActivity>,
          cancellationToken: CancellationToken) =
        async {
            do! dialog.RunAsync(turnContext, accessors.DialogState, cancellationToken) |> Async.AwaitTask
        } |> Async.StartAsTask :> Task