open System
open System.Diagnostics
open System.Text
open System.Web
open BetterRead.Infra
open HtmlAgilityPack

let encodingBuilder (name:string) =
    Encoding.GetEncoding name

let htmlWebFactory encoding =
    HtmlWeb (OverrideEncoding = encoding)

let getBookId url =
    let uri = Uri url
    let query = HttpUtility.ParseQueryString uri.Query
    match Int32.TryParse(query.Get "id") with
    | (true, id) -> Some id
    | _ -> failwith "todo"

[<EntryPoint>]
let main _ =
    Encoding.RegisterProvider CodePagesEncodingProvider.Instance
    Console.OutputEncoding <- Encoding.UTF8
    
    let url = "http://loveread.ec/view_global.php?id=81173"
    let bookId = getBookId url |> Option.get
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