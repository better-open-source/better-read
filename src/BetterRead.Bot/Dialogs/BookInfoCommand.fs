namespace BetterRead.Bot.Dialogs

open System
open BetterRead.Bot.Domain.Book

module BookInfoCommandModule =
    let (|BookInfoCommand|_|) str =
        match Uri.TryCreate(str, UriKind.Absolute) with
        | (true, uri) -> getBookId uri
        | _ -> None