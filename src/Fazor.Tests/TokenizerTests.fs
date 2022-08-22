open NUnit.Framework
open System
open Fazor

let parseAndPrint h =
    printf "source=%s" h
    printf "document=%s"
        (string (Tokenizer.tokenize h))
    printf "\n\n===\n\n"

[<Test>]
let testFormatDocXml () =
    parseAndPrint """@model XXX.YYY
@inject AAA.BBB ooo
"""

    parseAndPrint """
@model XXX.YYY
@inject AAA.BBB ooo
"""

    raise (NotImplementedException())
    ()

