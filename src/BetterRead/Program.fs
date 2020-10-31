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
    
    let bookInfo = BookInfoParser.parse bookId htmlWeb |> Async.RunSynchronously
    bookInfo |> printfn "%A"
    0