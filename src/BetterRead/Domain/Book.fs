module BetterRead.Domain.Book
    
open System

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
