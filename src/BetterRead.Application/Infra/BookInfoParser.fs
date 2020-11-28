module BetterRead.Application.Infra.BookInfoParser 

open System

open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack

open BetterRead.Common
open BetterRead.Common.HttpFetcher
open BetterRead.Application.Domain.Book

let private getAttributeValue (node:HtmlNode) (attr:string) =
    node.GetAttributeValue(attr, String.Empty)

let private extractTitleWith (node:HtmlNode) (pattern:string)  =
    node.QuerySelectorAll "a"
    |> Seq.filter (fun node -> (getAttributeValue node "href").Contains(pattern))
    |> Seq.map (fun node -> getAttributeValue node "title")
    |> Seq.tryHead
    
let private extractBookName bookId (node:HtmlNode) =
    let bookUrlPath = sprintf "read_book.php?id=%d" bookId
    extractTitleWith node <| bookUrlPath

let private extractAuthor (node:HtmlNode) =
    extractTitleWith node <| "author="

let parseBookInfo (htmlWeb:HtmlWeb) bookId = async {
    let url = BookUrls.bookUrl bookId
    let! htmlDocument = htmlWeb.LoadFromWebAsync url |> Async.AwaitTask
    let! image = downloadContent (BookUrls.bookCover bookId |> Uri)
    
    let documentNode = htmlDocument.DocumentNode
    
    return {
        Id = bookId
        Name = extractBookName bookId documentNode |> Option.defaultValue "N/F"
        Author = extractAuthor documentNode |> Option.defaultValue "N/F"
        Url = Uri <| url
        Image = image }
}
