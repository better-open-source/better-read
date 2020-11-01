open System
open System.Diagnostics
open System.Text
open BetterRead.Infra
open HtmlAgilityPack

let encodingBuilder (name:string) =
    Encoding.GetEncoding name

let htmlWebFactory encoding =
    HtmlWeb (OverrideEncoding = encoding)

[<EntryPoint>]
let main argv =
    Encoding.RegisterProvider CodePagesEncodingProvider.Instance
    Console.OutputEncoding <- Encoding.UTF8
    
    let bookId = 81173
    let htmlWeb = htmlWebFactory <| encodingBuilder "windows-1251"
    
    let timer = Stopwatch()
    timer.Start()
    
    let bookInfo = BookInfoParser.parse htmlWeb bookId |> Async.RunSynchronously
    bookInfo |> printfn "%A"
    
    let bookPages = BookSheetsParser.parse htmlWeb bookId |> Async.RunSynchronously
    bookPages |> printfn "%A"
    
    timer.Stop()
    printf "sec: %d, ms: %d" timer.Elapsed.Seconds timer.Elapsed.Milliseconds
    0