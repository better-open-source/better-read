module BetterRead.Common.AsyncExtensions

module Async =
    let traverseOpt = function
        | Some x -> async {
            let! result = x
            return Some result }
        | None   -> None |> async.Return
    
    let map f xAsync = async {
        let! x = xAsync
        return f x
    }