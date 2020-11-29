module BetterRead.Application.Infra.GetBook

open System
open System.IO
open Azure.Storage.Blobs

open BetterRead.Application.Domain
open BetterRead.Application.Parsers.BookInfoParser
open BetterRead.Application.Parsers.BookSheetsParser
open BetterRead.Application.Infra.BookBuilder
open BetterRead.Application.Infra.HtmlWebFactory
open BetterRead.Common.AsyncExtensions
open FSharp.Control

type BlobBook =
    { Name  : string
      Url   : Uri }

let checkIfBlobExistsAsync (containerClient : BlobContainerClient) fileName =
    async {
        return!
            containerClient.GetBlobsAsync(prefix = fileName) |>
            AsyncSeq.ofAsyncEnum |>
            AsyncSeq.exists (fun x -> x.Name = fileName)
    }

let buildBookAsync id info =
    async {
        let! sheets = parseSheets htmlWeb id
        return { Info = info; Sheets = sheets }
    }
    
let fetchBlobBook id (containerClient : BlobContainerClient) =
    async {
        do! containerClient.CreateIfNotExistsAsync() |> Async.AwaitTask |> Async.Ignore
        let! info = parseBookInfo htmlWeb id
        let fileName = sprintf "%s/%s.docx" info.Author info.Name
        
        match! checkIfBlobExistsAsync containerClient fileName with
        | true ->
            let blobClient = containerClient.GetBlobClient(fileName)
            return Some { Name = info.Name; Url = blobClient.Uri }
        | false ->
            let! book = buildBookAsync id info
            use content = generateDocument book |> (fun bytes -> new MemoryStream(bytes))
            do! containerClient.UploadBlobAsync(fileName, content) |> Async.AwaitTask |> Async.Ignore
            let blobClient = containerClient.GetBlobClient(fileName)
            return Some { Name = info.Name; Url = blobClient.Uri }
    }

