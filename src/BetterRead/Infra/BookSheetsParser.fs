module BetterRead.Infra.BookSheetsParser

open System
open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack

open BetterRead.Configuration.AsyncExtensions
open BetterRead.Configuration
open BetterRead.Domain.Book

let private getHtmlNodeAsync (htmlWeb:HtmlWeb) bookId pageId = async {
    let url = BookUrls.bookPage bookId pageId
    let! document = htmlWeb.LoadFromWebAsync url |> Async.AwaitTask
    return (pageId, document.DocumentNode)
}

let private getPagesCount (node:HtmlNode) =
    node.QuerySelectorAll "div.navigation > a"
    |> Seq.choose (fun n ->
           
           match Int32.TryParse(n.InnerHtml) with
           | (true, x) -> Some x
           | _ -> None)
    |> Seq.max

let private (|Attr|_|) (pattern:string) (attrs:HtmlAttributeCollection)  =
    attrs |> Seq.tryFind (fun x -> x.Value = pattern || x.Value.Contains pattern)

let private parseNode (node:HtmlNode) =
    match node.Attributes with
    | Attr "take_h1" _ -> Header node.InnerText
    | Attr "MsoNormal" _ -> Paragraph node.InnerText
    | Attr "img/photo_books/" img -> Image <| BookUrls.baseUrl + "/" + img.Value
    | _ -> Unknown

let private getPageNodes (node:HtmlNode) =
    match node.QuerySelectorAll "div.MsoNormal" |> Seq.tryExactlyOne with
    | Some divNode ->
        divNode.ChildNodes |> Seq.map parseNode
        |> Seq.filter (function | Unknown -> false | _ -> true)
        |> Some
    | None -> None

let parseSheets (htmlWeb:HtmlWeb) bookId = async {
    let concretePage = getHtmlNodeAsync htmlWeb bookId
    let! (_, firstPage) = concretePage 1
    
    return!
        [1 .. getPagesCount firstPage]
        |> Seq.map concretePage
        |> Async.Parallel
        |> Async.map (Array.choose (fun (pageId, pageNode) ->
                        match pageNode |> getPageNodes with
                        | Some nodes -> Some {Id = pageId; SheetContents = nodes |> Seq.toArray}
                        | _ -> None))
}