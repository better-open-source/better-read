namespace BetterRead.Bot.Bots

open System.Threading
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Extensions.Logging

type DialogBot<'dialog when 'dialog :> Dialog>
    ( dialog : Dialog,
      logger : ILogger<DialogBot<'dialog>>) =
    inherit ActivityHandler()
    