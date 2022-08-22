namespace Fazor
open FParsec
open System

module Tokenizer =
    type Positioned<'T> = { Value: 'T; Start: int64; End: int64 }

    type HtmlAttribute = string * string

    type FazorToken =
        | DirectiveToken of Positioned<string * string>
        | WhitespaceToken of Positioned<string>
        | WhitespaceCharsToken of Positioned<char list>
        | HtmlToken of Positioned<string>

    let positioned (p: Parser<'T, 'U>) : Parser<Positioned<'T>, 'U> =
        // Get the position before and after parsing
        pipe3 getPosition p getPosition <| fun start value finish ->
            { Value = value
              Start = start.Index
              End = if finish.Column > 1L then finish.Index - 1L else finish.Index
            }

    let isIdentifierChar c = isLetter c || isDigit c || c = '_' || c = '.'
    let identifier = many1Satisfy isIdentifierChar

    let pws = many (pchar ' ' <|> pchar '\t')
    //let pws1 = many1 (pchar ' ' <|> pchar '\t')
    //let pws1AndIdentifier = pws1 >>. pidentifier
    //let pmodelDeclaration = pstring "@model" >>. pws1AndIdentifier .>> restOfLine true |> positioned |>> ModelDeclaration
    //let pinjectDeclaration = pstring "@inject" >>. pws1AndIdentifier .>>. pws1AndIdentifier .>> restOfLine true |> positioned |>> InjectDeclaration

    let pemptyLine = pws .>> newline |> positioned |>> WhitespaceCharsToken

    let pdirective = pstring "@" >>. identifier .>>. restOfLine true |> positioned |>> DirectiveToken

    let isWhitespaceChar c = c = ' ' || c = '\t' || c = '\n' || c = '\r'
    let pwhitespace = many1Satisfy isWhitespaceChar |> positioned |>> WhitespaceToken

    //let htmlNode =
    //  pstring "<html>" >>. pws .>> pstring "</html>" |> positioned
    //  |>> (fun x -> HtmlElement { Value=("html", [], []); Start=x.Start; End=x.End })

    let pdocument =
        (many (choice [pdirective; pemptyLine])) .>>. (many pwhitespace)

    let tokenize cshtml =
        match run pdocument cshtml with
        | Success (result, _, _) -> result
        | Failure (error, _, _) -> Exception (sprintf "Error: %A" error) |> raise
