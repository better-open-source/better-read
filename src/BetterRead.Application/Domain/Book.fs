module BetterRead.Application.Domain
    
open System
open System.Web

open BetterRead.Common.BookUrls

type ImageData = byte[] option * Uri

type SheetContent =
    | Header     of string
    | Paragraph  of string
    | Image      of ImageData
    | Unknown

type Sheet =
    { Id       : int
      Contents : SheetContent[] }

type BookInfo =
    { Id      : int
      Name    : string
      Author  : string
      Url     : Uri
      Image   : ImageData }

type Book =
    { Info    : BookInfo
      Sheets  : Sheet[] }

let getBookId (uri : Uri) =
    if uri.AbsoluteUri.Contains(baseUrl) then
        let query = HttpUtility.ParseQueryString uri.Query
        match Int32.TryParse(query.Get "id") with
        | (true, id) -> Some id
        | _          -> None
    else
        None
    