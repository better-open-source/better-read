module BetterRead.Common.HttpFetcher

open System
open System.IO
open System.Net.Http
open System.Threading

let private toMemoryStream (stream:Stream) = async {
    let ms = new MemoryStream()
    do! stream.CopyToAsync ms |> Async.AwaitTask
    do! stream.FlushAsync CancellationToken.None |> Async.AwaitTask
    return ms
}
    
let downloadContent (url:Uri) = async {
    try
        use client = new HttpClient()
        let! stream = client.GetStreamAsync url |> Async.AwaitTask
        let! ms = toMemoryStream stream
        return (ms.ToArray() |> Option.Some, url)
    with
    | _ -> return (None, url) 
}
