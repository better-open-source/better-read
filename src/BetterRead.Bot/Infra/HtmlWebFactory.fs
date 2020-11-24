module BetterRead.Bot.Infra.HtmlWebFactory

open System.Text
open HtmlAgilityPack

let private encodingBuilder (name:string) =
    Encoding.GetEncoding name

let private htmlWebFactory encoding =
    HtmlWeb (OverrideEncoding = encoding)

let htmlWeb = htmlWebFactory <| encodingBuilder "windows-1251"
