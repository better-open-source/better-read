module BetterRead.Bot.Configuration.AsyncExtensions

module Async =
    let traverseOpt = function
        | Some y -> async {
            let! result = y
            return Some result }
        | None   -> None |> async.Return
    
    let map f xAsync = async {
        let! x = xAsync
        return f x
    }