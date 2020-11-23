module BetterRead.Bot.Configuration.AsyncExtensions

module Async =
    let map f xAsync = async {
        let! x = xAsync
        return f x
    }
    
    let revMap xAsync f = async {
        let! x = xAsync
        return f x
    }
    
    let retn x = async {
        return x
    }

    let apply fAsync xAsync = async {
        let! fChild = Async.StartChild fAsync
        let! xChild = Async.StartChild xAsync

        let! f = fChild
        let! x = xChild 

        return f x 
    }
    
    let applyOne xAsynx = async {
        return! xAsynx
    }

    let bind f xAsync = async {
        let! x = xAsync 
        return! f x
    }