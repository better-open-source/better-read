module BetterRead.Application.Infra.GetBook

open System
open Azure.Storage.Blobs

type BlobBook =
    { Name: string
      Url: Uri }

//let! info = parseBookInfo htmlWeb id
//let! sheets = parseSheets htmlWeb id
//do! sendDocument stepContext.Context {Info = info; Sheets = sheets}
let getBlobBook id (blobCli: BlobContainerClient) = async {
    return Some { Name = ""; Url = Uri("") }
}

