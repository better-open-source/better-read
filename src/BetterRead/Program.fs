open System
open System.Diagnostics
open System.IO
open System.Text
open System.Web

open HtmlAgilityPack

open BetterRead.Domain.Book
open BetterRead.Infra.BookBuilder
open BetterRead.Infra.BookInfoParser
open BetterRead.Infra.BookSheetsParser

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

let mainAsync = async {
    let url = "http://loveread.ec/view_global.php?id=81173"
    let bookId = getBookId url |> Option.get
    let htmlWeb = htmlWebFactory <| encodingBuilder "windows-1251"
    
    printfn "Fetching book from url: %s\n..." url
    
    let timer = Stopwatch()
    timer.Start()
    
    let! bookInfo = parseBookInfo htmlWeb bookId
    let! bookSheets = parseSheets htmlWeb bookId
    
    let book = {
        Info = bookInfo
        Sheets = bookSheets
    }
    
    timer.Stop()
    
    printfn "%A" book.Info
    printfn "sheets %d" book.Sheets.Length
    printfn "elapsed ms: %d\n" timer.Elapsed.Milliseconds
    
    timer.Reset()
    timer.Start()
    
    printfn "Generating book..."
    let content = generateDocument book
    File.WriteAllBytes(sprintf "%s.docx" book.Info.Name, content)
    
    printfn "elapsed ms: %d\n" timer.Elapsed.Milliseconds
    
    timer.Stop()
    
    return 0
}

[<EntryPoint>]
let main _ =
    Encoding.RegisterProvider CodePagesEncodingProvider.Instance
    Console.OutputEncoding <- Encoding.UTF8
    
    mainAsync |> Async.RunSynchronously
    