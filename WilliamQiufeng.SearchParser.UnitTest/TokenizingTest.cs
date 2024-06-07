namespace WilliamQiufeng.SearchParser.UnitTest;

[TestFixture("", new object[] { })]
[TestFixture("a", new object[] { new object[] { TokenKind.PlainText, "a", 0, "a" } })]
[TestFixture("a b", new object[]
{
    new object[] { TokenKind.PlainText, "a", 0, "a" },
    new object[] { TokenKind.PlainText, "b", 2, "b" }
})]
[TestFixture("multichar words", new object[]
{
    new object[] { TokenKind.PlainText, "multichar", 0, "multichar" },
    new object[] { TokenKind.PlainText, "words", 10, "words" }
})]
[TestFixture(new[] { "key" }, "k ke key keya ka", new object[]
{
    new object[] { TokenKind.Key, "k", 0, "key" },
    new object[] { TokenKind.Key, "ke", 2, "key" },
    new object[] { TokenKind.Key, "key", 5, "key" },
    new object[] { TokenKind.PlainText, "keya", 9, "keya" },
    new object[] { TokenKind.PlainText, "ka", 14, "ka" }
})]
[TestFixture(new[] { "aaa", "aba", "aab", "bbc" }, "a ab aa aab bbc aac", new object[]
{
    new object[] { TokenKind.Key, "a", 0, "aaa" },
    new object[] { TokenKind.Key, "ab", 2, "aba" },
    new object[] { TokenKind.Key, "aa", 5, "aaa" },
    new object[] { TokenKind.Key, "aab", 8, "aab" },
    new object[] { TokenKind.Key, "bbc", 12, "bbc" },
    new object[] { TokenKind.PlainText, "aac", 16, "aac" }
})]
[TestFixture(new[] { "a1", "ab" }, "a1 1a 123 0012 8%", new object[]
{
    new object[] { TokenKind.Key, "a1", 0, "a1" },
    new object[] { TokenKind.PlainText, "1a", 3, "1a" },
    new object[] { TokenKind.Integer, "123", 6, 123 },
    new object[] { TokenKind.Integer, "0012", 10, 12 },
    new object[] { TokenKind.Percentage, "8%", 15, 8 },
})]
[TestFixture(new[] { "a1", "ab" }, "'a1\" 1a' 123", new object[]
{
    new object[] { TokenKind.String, "'a1\" 1a'", 0, "a1\" 1a" },
    new object[] { TokenKind.Integer, "123", 9, 123 },
})]
[TestFixture("'aa", new object[]
{
    new object[] { TokenKind.PlainText, "'aa", 0, "'aa" },
})]
[TestFixture("> >= = == < <= | / ! : !=", new object[]
{
    new object[] { TokenKind.MoreThan, ">", 0, default! },
    new object[] { TokenKind.MoreThanOrEqual, ">=", 2, default! },
    new object[] { TokenKind.Equal, "=", 5, default! },
    new object[] { TokenKind.Equal, "==", 7, default! },
    new object[] { TokenKind.LessThan, "<", 10, default! },
    new object[] { TokenKind.LessThanOrEqual, "<=", 12, default! },
    new object[] { TokenKind.Or, "|", 15, default! },
    new object[] { TokenKind.Or, "/", 17, default! },
    new object[] { TokenKind.Not, "!", 19, default! },
    new object[] { TokenKind.Contains, ":", 21, default! },
    new object[] { TokenKind.NotEqual, "!=", 23, default! },
})]
[TestFixture(new[] { "a", "b" }, "a>3 a < 5%", new object[]
{
    new object[] { TokenKind.Key, "a", 0, "a" },
    new object[] { TokenKind.MoreThan, ">", 1, default! },
    new object[] { TokenKind.Integer, "3", 2, 3 },
    new object[] { TokenKind.Key, "a", 4, "a" },
    new object[] { TokenKind.LessThan, "<", 6, default! },
    new object[] { TokenKind.Percentage, "5%", 8, 5 },
})]
[TestFixture("123.456 0.4 .12 12.34.56 1.3s", new object[]
{
    new object[] { TokenKind.Real, "123.456", 0, 123.456 },
    new object[] { TokenKind.Real, "0.4", 8, 0.4 },
    new object[] { TokenKind.Real, ".12", 12, 0.12 },
    new object[] { TokenKind.PlainText, "12.34.56", 16, "12.34.56" },
    new object[] { TokenKind.PlainText, "1.3s", 25, "1.3s" },
})]
[TestFixture("12m36s 1hr 6s7s 1h2t 3h4 3hours4minutes12seconds 3:45 5: 1:2:3:4 1:34s 1::1 1ms", new object[]
{
    new object[] { TokenKind.TimeSpan, "12m36s", 0 },
    new object[] { TokenKind.TimeSpan, "1hr", 7 },
    new object[] { TokenKind.PlainText, "6s7s", 11 },
    new object[] { TokenKind.PlainText, "1h2t", 16 },
    new object[] { TokenKind.PlainText, "3h4", 21 },
    new object[] { TokenKind.TimeSpan, "3hours4minutes12seconds", 25 },
    new object[] { TokenKind.TimeSpan, "3:45", 49 },
    new object[] { TokenKind.PlainText, "5:", 54 },
    new object[] { TokenKind.PlainText, "1:2:3:4", 57 },
    new object[] { TokenKind.PlainText, "1:34s", 65 },
    new object[] { TokenKind.PlainText, "1::1", 71 },
    new object[] { TokenKind.PlainText, "1ms", 76 },
})]
[TestFixture(new[] { "mode" }, new[] { "quaver", "etterna", "osu", "malody" }, "m=q/o/m", new object[]
{
    new object[] { TokenKind.Key, "m", 0, "mode" },
    new object[] { TokenKind.Equal, "=", 1, default! },
    new object[] { TokenKind.Enum, "q", 2, "quaver" },
    new object[] { TokenKind.Or, "/", 3, default! },
    new object[] { TokenKind.Enum, "o", 4, "osu" },
    new object[] { TokenKind.Or, "/", 5, default! },
    new object[] { TokenKind.Enum, "m", 6, "malody" },
})]
[TestFixture(new[] { "mode" }, new[] { "quaver", "etterna", "osu", "malody" }, "m=mo", new object[]
{
    new object[] { TokenKind.Key, "m", 0, "mode" },
    new object[] { TokenKind.Equal, "=", 1, default! },
    new object[] { TokenKind.Keyword, "mo", 2, default! },
})]
public class TokenizingTest
{
    private readonly Token[] _expectedTokens;
    private readonly Tokenizer _tokenizer;
    private readonly bool _checkValue = true;

    public TokenizingTest(string source, object[] expected)
    {
        _checkValue = expected.Cast<object[]>().All(o => o.Length >= 4);
        _expectedTokens = expected.Cast<object[]>()
            .Select(e => new Token((TokenKind)e[0], ((string)e[1]).AsMemory(), (int)e[2], _checkValue ? e[3] : null))
            .ToArray();
        _tokenizer = new Tokenizer(source);
    }

    public TokenizingTest(string[] keys, string source, object[] expected) : this(keys, [], source, expected)
    {
    }

    public TokenizingTest(string[] keys, string[] enums, string source, object[] expected) : this(source, expected)
    {
        foreach (var key in keys)
        {
            _tokenizer.KeywordTrie.Add(key, TokenKind.Key, key);
        }

        foreach (var @enum in enums)
        {
            _tokenizer.KeywordTrie.Add(@enum, TokenKind.Enum, @enum);
        }
    }

    [Test]
    public void Correct()
    {
        var tokens = _tokenizer.ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(_expectedTokens.Length + 1, Is.EqualTo(tokens.Length)); // Include End token
            Assert.That(tokens[^1].Kind, Is.EqualTo(TokenKind.End));
        });
        for (var i = 0; i < _expectedTokens.Length; i++)
        {
            TestContext.WriteLine($"Expected: {_expectedTokens[i]}, Found: {tokens[i]}");
            if (tokens[i].TryCollapseKeyword(_expectedTokens[i].Kind, false))
            {
                TestContext.WriteLine($"Collapsed into {tokens[i]}");
            }

            Assert.Multiple(() =>
            {
                Assert.That(_expectedTokens[i].Kind, Is.EqualTo(tokens[i].Kind));
                Assert.That(_expectedTokens[i].Segment.ToString(), Is.EqualTo(tokens[i].Segment.ToString()));
                Assert.That(_expectedTokens[i].Offset, Is.EqualTo(tokens[i].Offset));
                if (_checkValue && _expectedTokens[i].Value != null)
                    Assert.That(_expectedTokens[i].Value, Is.EqualTo(tokens[i].Value));
            });
        }
    }
}