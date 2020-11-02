module BetterRead.Configuration.AsyncExtensions

module Async =
    let map f xAsync = async {
        let! x = xAsync
        return f x
    }
    
    let revMap xAsync f = async {
        let! x = xAsync
        return f x
    }