namespace BetterRead.Api.Controllers

open Microsoft.AspNetCore.Mvc

type HomeController() =
    inherit ControllerBase()
    
    [<HttpGet>]
    member __.Index() = JsonResult "Bot is running"