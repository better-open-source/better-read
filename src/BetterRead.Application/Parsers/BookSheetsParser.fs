module BetterRead.Application.Parsers.BookSheetsParser

open System

open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack

open BetterRead.Common
open BetterRead.Common.AsyncExtensions
open BetterRead.Application.Domain

let private getHtmlNodeAsync (htmlWeb : HtmlWeb) bookId pageId =
    async {
        let url = BookUrls.bookPage bookId pageId
        let! document = htmlWeb.LoadFromWebAsync url |> Async.AwaitTask
        return (pageId, document.DocumentNode)
    }

let private getPagesCount (pageNode : HtmlNode) =
    pageNode.QuerySelectorAll "div.navigation > a"
    |> Seq.choose (fun node ->
           match Int32.TryParse(node.InnerHtml) with
           | (true, pageId) -> Some pageId
           | _              -> None)
    |> Seq.max

let private (|Attr|_|) pattern (attrs : HtmlAttributeCollection)  =
    attrs |> Seq.tryFind (fun x -> x.Value = pattern || x.Value.Contains pattern)

let private parseNode (node : HtmlNode) =
    match node.Attributes with
    | Attr "take_h1" _            -> Header node.InnerText |> async.Return
    | Attr "MsoNormal" _          -> Paragraph node.InnerText |> async.Return
    | Attr "img/photo_books/" img -> BookUrls.baseUrl + "/" + img.Value
                                     |> Uri |> HttpFetcher.downloadContent
                                     |> Async.map Image
    | _                           -> Unknown |> async.Return

let private getPageNodes (node : HtmlNode) =
    match node.QuerySelectorAll "div.MsoNormal" |> Seq.tryExactlyOne with
    | Some divNode -> divNode.ChildNodes |> Seq.map parseNode |> Some
    | None -> None

let getSheets (pageId, pageNode) =
    match pageNode |> getPageNodes with
    | Some nodes -> nodes
                    |> Async.Parallel
                    |> Async.map (fun contents -> Some {Id = pageId; Contents = contents})
    | None       -> async.Return None
    
let parseSheets (htmlWeb : HtmlWeb) bookId =
    async {
        let concretePage = getHtmlNodeAsync htmlWeb bookId
        let! (_, firstPage) = concretePage 1
        let! pages =
            [| 1 .. getPagesCount firstPage |]
            |> Array.map concretePage
            |> Async.Parallel
        
        return! pages
            |> Array.map getSheets
            |> Async.Parallel
            |> Async.map (Array.choose id)
    }