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

let getSheetsCount (node:HtmlNode) =
    node.QuerySelectorAll "div.navigation > a"
    |> Seq.map (fun n ->
           match System.Int32.TryParse(n.InnerHtml) with
           | (true, x) -> Some x
           | _ -> None)
    |> Seq.filter (fun x -> x.IsSome)
    |> Seq.max

let parse (htmlWeb:HtmlWeb) bookId =
    async {
        let pageFactory = getPageFactory htmlWeb bookId
        let! (_, firstNode) = pageFactory 1
        
    }