module BetterRead.Bot.Infra.BookSheetsParser

open System

open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack

open BetterRead.Bot.Infra.HttpFetcher
open BetterRead.Bot.Configuration.AsyncExtensions
open BetterRead.Bot.Configuration
open BetterRead.Bot.Domain.Book

let retn = Async.retn

let private getHtmlNodeAsync (htmlWeb:HtmlWeb) bookId pageId = async {
    let url = BookUrls.bookPage bookId pageId
    let! document = htmlWeb.LoadFromWebAsync url |> Async.AwaitTask
    return (pageId, document.DocumentNode)
}

let private getPagesCount (pageNode:HtmlNode) =
    pageNode.QuerySelectorAll "div.navigation > a"
    |> Seq.choose (fun node ->
           match Int32.TryParse(node.InnerHtml) with
           | (true, pageId) -> Some pageId
           | _              -> None)
    |> Seq.max

let private (|Attr|_|) pattern (attrs:HtmlAttributeCollection)  =
    attrs |> Seq.tryFind (fun x -> x.Value = pattern || x.Value.Contains pattern)

let private parseNode (node:HtmlNode) =
    match node.Attributes with
    | Attr "take_h1" _            -> Header node.InnerText |> retn
    | Attr "MsoNormal" _          -> Paragraph node.InnerText |> retn
    | Attr "img/photo_books/" img -> BookUrls.baseUrl + "/" + img.Value
                                     |> Uri |> downloadContent
                                     |> Async.map Image
    | _                           -> Unknown |> retn

let private getPageNodes (node:HtmlNode) =
    match node.QuerySelectorAll "div.MsoNormal" |> Seq.tryExactlyOne with
    | Some divNode -> divNode.ChildNodes |> Seq.map parseNode |> Some
    | None -> None

let getSheets (pageId, pageNode) =
    match pageNode |> getPageNodes with
    | Some nodes -> nodes
                    |> Async.Parallel
                    |> Async.map (fun contents -> Some {Id = pageId; SheetContents = contents})
    | None       -> retn None
    
let parseSheets (htmlWeb:HtmlWeb) bookId = async {
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