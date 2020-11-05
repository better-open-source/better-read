module BetterRead.Infra.BookBuilder

open System.IO
open System.Net.Http
open System.Threading

open Xceed.Document.NET
open Xceed.Words.NET

let toMemoryStream (stream:Stream) = async {
    let ms = new MemoryStream()
    do! stream.CopyToAsync ms |> Async.AwaitTask
    do! stream.FlushAsync CancellationToken.None |> Async.AwaitTask
    return ms
}
    
let downloadImage (url:string) = async {
    use client = new HttpClient()
    let! stream = client.GetStreamAsync url |> Async.AwaitTask
    let! ms = toMemoryStream stream
    return ms.ToArray
}

let buildHeader content (doc:DocX) =
    let p = doc.InsertParagraph().Append(content)
                .FontSize(20.0).Bold()
                .SpacingBefore(15.0).SpacingAfter(13.0)
    p.Alignment <- Alignment.center
    ignore
    
let buildParagraph content (doc:DocX) =
    let p = doc.InsertParagraph().Append(content).SpacingAfter(5.5)
    p.IndentationFirstLine <- (float32(1))
    p.Alignment <- Alignment.both
    ignore
