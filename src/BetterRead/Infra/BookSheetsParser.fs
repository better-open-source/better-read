module BetterRead.Infra.BookSheetsParser

open System
open BetterRead.Configuration.AsyncExtensions
open BetterRead.Configuration
open BetterRead.Domain.Book
open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack

let private (<*>) = Async.revMap

let private getHtmlNodeAsync (htmlWeb:HtmlWeb) bookId pageId =
    async {
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

let private getPageWithNodes (_, node:HtmlNode) =
    match node.QuerySelectorAll "div.MsoNormal" |> Seq.tryExactlyOne with
    | Some divNode -> Some divNode.ChildNodes
    | None -> None

let private (|MatchAttr|_|) (pattern:string) (attrs:HtmlAttributeCollection)  =
    attrs |> Seq.tryFind (fun x -> x.Value = pattern || x.Value.Contains pattern)

let private parseNode (node:HtmlNode) =
    match node.Attributes with
    | MatchAttr "take_h1" _ -> Header node.InnerText
    | MatchAttr "MsoNormal" _ -> Paragraph node.InnerText
    | MatchAttr "img/photo_books/" attr -> Image <| BookUrls.baseUrl + "/" + attr.Value
    | _ -> Unknown

let parse (htmlWeb:HtmlWeb) bookId = async {
    let concretePage = getHtmlNodeAsync htmlWeb bookId
    let! (_, firstPageNode) = concretePage 1
    let pagesCount = getPagesCount firstPageNode
    
    return!
        [1..pagesCount]
        |> Seq.map concretePage
        |> Async.Parallel
        <*> Seq.choose getPageWithNodes
        <*> Seq.collect (fun x -> x)
        <*> Seq.map parseNode
        <*> Seq.filter (function | Unknown -> false | _ -> true)
        <*> Seq.toArray
}