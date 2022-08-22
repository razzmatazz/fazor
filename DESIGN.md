# Design

There are several parts of Fazor library that are used to compose a layered 
design. Those layers can be used separately too.

- I. Tokenizer; parses .cshtml into tokens, produces a positioned token list;
- II. Model; a hierarchical model of .cshtml file, w/ interspersed C# directives;
- III. Compiler; produces a C# code (class) from the model that receives a @Model+other context as an input;
- IV. Evaluator; takes compiled model and can evaluate it to produce with given model/context.

## Tokenizer
.cshtml contains regular html file contents, with interpolated C# directives that are used
to insert logic into generation code.
### Tokens
Tokenizing a .cshtml produces a list of FazorToken's:
- `WhitespaceToken` - ignored by the compiler, does not produce any logic or output html;
- `HtmlToken` - a parsed body of html text that will be produced verbatim on the output html (considering the semantics of outer C# logic);
- `CSharpDirectiveToken` - a parsed `@<directive> <params>` directive from .cshtml header;
- `CSharpExpressionToken` - an inline C# expression in html body, e.g. `@Model.Property` or `@(1 + 1)`; -- result of expression is emited into output html, after escaping;
- `CSharpBlockToken` - a block of C# body/logic that is inserted into razor output C# code to be executed in runtime.

## Parser
Tokens from tokenizer are parsed to produce a hierarchical model of the .cshtml template.

### SourcePositioned<'t>
Defines a value that is source-positioned, i.e. provides where on the source
file the original token resides, so we can:
- provide error messages;
- provide compiled -> original file mapping; (e.g. for LSP server functionality where we want to provide autocompletion on source file based on the compiled file info);

### RazorDocument
`RazorDocument` has two properties:
- Directives: `RazorDirective list`
- Root: `RazorNode`

### RazorDirective
`RazorDirective` is a discriminated union of:
- `RazorModelDirective`
- `RazorInjectDirective`
- *TODO: more directives*

### RazorNode
```fsharp
type RazorCSharpBlockNodeParams = {
    Head: SourcePositioned<string> option
    Body: SourcePositioned<string>
    ElseBody: SourcePositioned<string> option
}
and RazorCompositeNodeParams = {
    Subnodes: RazorNode list 
}
and RazorNode =
     | RazorCompositeNode of RazorCompositeNodeParams
     | RazorOutputNode of SourcePositioned<string>
     | RazorCSharpBlockNode of RazorCSharpBlockNodeParams
     | RazorCSharpExpressionNode SourcePositioned<string>
```

### Example model

Given input cshtml:
```
@model Project.Models.IndexViewModel
@inject Ilocalizator localizator
<html>
    <body>
        <ul>
            @foreach (var item in Model.Items)
            {
                <li>
                    @if (item.BoolProperty)
                    {
                        <text>YES</text>
                    }
                    else 
                    {
                        <text>NO</text>
                    }
                </li>
            }
        </ul>
        
@{
    var stringValue = "<hello world>";
}
        <div>@stringValue</div>
    </body>
</html>
```

Parser produces the following `RazorDocument` (model):
```fsharp
{
    Directives=[
        { Type="Project.Models.IndexViewModel" } : RazorModelDirective,
        { Type="ILocalizator"
          Variable="localizator" } : RazorInjectDirective,
    ] : RazorDirective list,

    Root={
        Subnodes=[
            { Output     = { Value="<html>\n<body>\n<ul>"; Start=0; End=20 } }: RazorOutputNode

            { 
              Head       = Some { Value="foreach (var item in Model.Items)"; Start=21; End=40 }
              Body       = { Subnodes=[
                                 { Output={ Value=\n<li>\n"; Start=41; End=51 } }: RazorOutputNode,

                                 {
                                   Head     = Some { Value="if (item.BoolProperty)"; Start=s; End=e }
                                   Body     = { Output={ Value="\nYES\n"; Start=s; End=e } } : RazorOutputNode
                                   ElseBody = Some { Output={ Value="\NO\n"; Start=s; End=e } } : RazorOutputNode
                                 } : RazorCSharpBlockNode,

                                 { Output={ Value="\n</li>\n"; Start=s; End=e } }: RazorOutputNode,
                             ]
                           } : RazorCompositeNode
              ElseBody   = None: RazorNode option
            } : RazorCSharpBlockNode

            { Output     = { Value="\n</ul>\n\n"; Start=s; End=e } }: RazorOutputNode,

            {
              Head       = None
              Body       = { Value="\nvar stringValue = \"; Start=s; End=e }<hello world>\";\n"
              ElseBody   = None
            } : RazorCsharpBlockNode,

            { Output     = { Value="<div>"; Start=s; End=e } } : RazorOutputNode,

            { Expression = { Value="stringValue"; Start=s; End=e } } : RazorCSharpExpressionNode,

            { Output     = { Value="</div>\n</body>\n<html>\n"; Start=s; End=e } } : RazorOutputNode,
        ]
        : RazorNode list
    }
    : RazorCompositeNode,
}
: RazorDocument
```

## Compiler
Compiler takes `RazorDocument` and produces C# class that corresponds to the given `RazorDocument`.

Implementation walks the document hierarchically and produces C# code using Roslyn.

*TODO*: HOW?

*TODO*: Example C# output given example `RazorDocument`?

## Evaluator
*TODO*
