namespace BetterRead.Bot.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Integration.AspNet.Core

[<Route("api/messages")>]
[<ApiController>]
type BotController
    ( adapter : IBotFrameworkHttpAdapter,
      bot     : IBot ) =
    inherit ControllerBase()

    [<HttpGet>]
    [<HttpPost>]
    member __.PostAsync() = async {
        return! adapter.ProcessAsync(__.Request, __.Response, bot) |> Async.AwaitTask
    }