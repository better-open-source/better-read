module BetterRead.Domain.Book
    
open System

type SheetContent =
    | Header of string
    | Paragraph of string
    | Image of string
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
    Image: Uri
}

type Book = {
    Info: BookInfo
    Sheets: Sheet[]
}