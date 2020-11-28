namespace BetterRead.Bot.StateAccessors

open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs

type BotStateAccessors(conversationState: ConversationState) =
    let dialogStateAccessor = conversationState.CreateProperty<DialogState> "BotStateAccessors.DialogState"
    
    member val ConversationState = conversationState with get
    member val DialogState = dialogStateAccessor with get