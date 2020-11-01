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
    
    let bookId = 81173
    let htmlWeb = htmlWebFactory <| encodingBuilder "windows-1251"
    
    let bookInfo = BookInfoParser.parse htmlWeb bookId |> Async.RunSynchronously
    bookInfo |> printfn "%A"
    
    let bookPages = BookSheetsParser.parse htmlWeb bookId |> Async.RunSynchronously
    bookPages |> printfn "%A"
    0