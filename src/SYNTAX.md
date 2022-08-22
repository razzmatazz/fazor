# RAZOR document (cshtml) syntax & semantics

TODO: this document is incomplete.

## Interpolation

There are several rules for interpolation

### Interpolation / directives
There can be one or more directives at the start of the file, beginning with '@' symbol, e.g. `@model <ClassName>`; there may be empty lines inbetween directives, at the start of the file or after them;

### Interpolation / C# expressions
There are multiple ways to inject C# code.

A. inline symbol ref: `@SymbolRef`, symbol ref may contain `.` or `[`/`]`
```
@Model.Array[0].Property
```

This emits the value of property (html-escaped) to output.

B. inline expression: `@(Expression)`
```
@(1 + 1)
```

This emits the value of expression (html-escaped) to output.

### Interpolation / C# blocks
Used to inject user C# code into the output-producing logic to execute in runtime.

#### C# control statement (if/while/until/for) or plain block

```
    @if (Model.Property == 1) {
      <div>one thing</div>
    } else {
      <div>another thing</div>
    }
```

..note how there is no @ before `else {`

Example for plain code block:
```
@{
    var aaa = 1;
    var bbb = aaa + 1;
}
```

This injects user C# code/logic into the output-producing logic to execute in runtime.

#### Escaping C# block

Writing a tag inside C# block enters html mode again.

```
@if (Model.Property == 1) {
   <span>Property equals 1</span>
}
```

You may also use the `<text></text>` tag to inject plain text w/o adding any tag to
output html, just the inner text of `<text>`:

```
@if (Model.Property == 1) {
   <text>Property equals 1</text>
}
```

### Interpolation / escaping @

Double @@ is used to escape interpolation and insert plain '@':
```
<a href="user@@email.com"></a>
```



