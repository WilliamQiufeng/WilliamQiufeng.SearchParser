# WilliamQiufeng.SearchParser

This is a simple parser that parses search criteria in a plain search query.

For example:

```csharp
var tokenizer = new Tokenizer("d>20 d<30 t: aaa !tag:sv abc");
var keys = new [] {"difficulty", "length", "title", "tag"};
foreach (var key in keys)
{
    tokenizer.KeywordTrie.Add(key, key);
}

var parser = new Parser(tokenizer.ToArray());
parser.Parse();

var terms = parser.GetPlainTextTerms(_source).ToArray();
var criteria = parser.SearchCriteria;
```

`terms` will contain `abc`, and the following criteria will be generated:
* `difficulty > 20`
* `difficulty < 20`
* `title contains aaa`
* `tag doesn't contain sv`
