module BetterRead.Application.Parsers.BookBuilder

open System.IO

open Xceed.Document.NET
open Xceed.Words.NET

open BetterRead.Application.Domain.Book

let private buildHeader content (doc:DocX) =
    let p = doc.InsertParagraph().Append(content)
                .FontSize(20.0).Bold()
                .SpacingBefore(15.0).SpacingAfter(13.0)
    p.Alignment <- Alignment.center
    ()
    
let private buildParagraph content (doc:DocX) =
    let p = doc.InsertParagraph().Append(content).SpacingAfter(5.5)
    p.IndentationFirstLine <- (float32 1)
    p.Alignment <- Alignment.both
    ()
    
let private buildImage (content:byte[] option) uri (doc:DocX) =
    match content with
    | Some data -> 
        use ms = new MemoryStream(data)
        let pic = doc.AddImage(ms).CreatePicture(float32 400, float32 400)
        let p = doc.InsertParagraph().AppendPicture(pic)
        p.Alignment <- Alignment.center
        ()
    | None ->
        let p = doc.InsertParagraph().Append(uri.ToString()).Italic()
        p.Alignment <- Alignment.center
        ()

let private dispatch = function
    | Header header        -> buildHeader header
    | Paragraph paragraph  -> buildParagraph paragraph
    | Image (content, url) -> buildImage content url
    | Unknown              -> (fun _ -> ())
    
let generateDocument book =
    let ms = new MemoryStream()
    let doc = DocX.Create(ms)
    doc.DifferentFirstPage <- true
    doc.AddFooters()
    
    let (content, uri) = book.Info.Image
    buildImage content uri doc
    
    book.Sheets
    |> Array.collect (fun sheet -> sheet.SheetContents)
    |> Array.iter (fun content -> dispatch content doc)
    
    doc.Save()
    ms.ToArray()
    