module BetterRead.Application.Infra.HtmlWebFactory

open System.Text

open HtmlAgilityPack

let private encodingBuilder (name : string) =
    Encoding.GetEncoding name

let private htmlWebFactory encoding =
    HtmlWeb (OverrideEncoding = encoding)

let htmlWeb = encodingBuilder "windows-1251" |> htmlWebFactory 
