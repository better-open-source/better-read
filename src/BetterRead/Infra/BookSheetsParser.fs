module BetterRead.Infra.BookSheetsParser

open BetterRead.Configuration
open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack;

let getHtmlNodeAsync (htmlWeb:HtmlWeb) bookId pageId =
    async {
        let url = BookUrls.bookPage bookId pageId
        let! document = htmlWeb.LoadFromWebAsync url |> Async.AwaitTask
        return (pageId, document.DocumentNode)
    }

let getPageFactory htmlWeb bookId =
    getHtmlNodeAsync htmlWeb bookId

let getPagesCount (node:HtmlNode) =
    node.QuerySelectorAll "div.navigation > a"
    |> Seq.map (fun n ->
           match System.Int32.TryParse(n.InnerHtml) with
           | (true, x) -> Some x
           | _ -> None)
    |> Seq.filter (fun x -> x.IsSome)
    |> Seq.map (fun x -> x.Value)
    |> Seq.max

let parse (htmlWeb:HtmlWeb) bookId =
    async {
        let concretePage = getPageFactory htmlWeb bookId
        let! (_, firstNode) = concretePage 1
        
        let pagesCount = getPagesCount firstNode
        let! t =
            [1..pagesCount]
            |> Seq.map (fun x -> concretePage x)
            |> Async.Parallel
        
        return 0
    }