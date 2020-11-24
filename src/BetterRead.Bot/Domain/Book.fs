module BetterRead.Bot.Domain.Book
    
open System
open System.Web

type ImageData = byte[] option * Uri

type SheetContent =
    | Header of string
    | Paragraph of string
    | Image of ImageData
    | Unknown

type Sheet = {
    Id: int
    SheetContents: SheetContent[]
}

type BookInfo = {
    Id: int
    Name: string
    Author: string
    Url: Uri
    Image: ImageData
}

type Book = {
    Info: BookInfo
    Sheets: Sheet[]
}

let getBookId (uri:Uri) =
    let query = HttpUtility.ParseQueryString uri.Query
    match Int32.TryParse(query.Get "id") with
    | (true, id) -> Some id
    | _ -> None
    