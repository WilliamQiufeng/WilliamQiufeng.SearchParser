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
[TestFixture(new[] { "a1", "ab" }, "a1 1a 123 0012", new object[]
{
    new object[] { TokenKind.Key, "a1", 0, "a1" },
    new object[] { TokenKind.PlainText, "1a", 3, "1a" },
    new object[] { TokenKind.Integer, "123", 6, 123 },
    new object[] { TokenKind.Integer, "0012", 10, 12 },
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
[TestFixture("> >= = == < <= | / ! :", new object[]
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
})]
[TestFixture("123.456 0.4 .12 12.34.56", new object[]
{
    new object[] { TokenKind.Real, "123.456", 0, 123.456 },
    new object[] { TokenKind.Real, "0.4", 8, 0.4 },
    new object[] { TokenKind.Real, ".12", 12, 0.12 },
    new object[] { TokenKind.PlainText, "12.34.56", 16, "12.34.56" },
})]
[TestFixture("12m36s 1hr 6s7s 3hours4minutes12seconds", new object[]
{
    new object[] { TokenKind.TimeSpan, "12m36s", 0 },
    new object[] { TokenKind.TimeSpan, "1hr", 7 },
    new object[] { TokenKind.PlainText, "6s7s", 11 },
    new object[] { TokenKind.TimeSpan, "3hours4minutes12seconds", 16 },
})]
public class TokenizingTest
{
    public Token[] ExpectedTokens;
    public string Source;
    public Tokenizer Tokenizer;
    public bool CheckValue = true;

    public TokenizingTest(string source, object[] expected)
    {
        Source = source;
        CheckValue = expected.Cast<object[]>().All(o => o.Length >= 4);
        ExpectedTokens = expected.Cast<object[]>()
            .Select(e => new Token((TokenKind)e[0], ((string)e[1]).AsMemory(), (int)e[2], CheckValue ? e[3] : null))
            .ToArray();
        Tokenizer = new Tokenizer(Source);
    }

    public TokenizingTest(string[] keys, string source, object[] expected) : this(source, expected)
    {
        foreach (var key in keys)
        {
            Tokenizer.KeywordTrie.Add(key, key);
        }
    }

    [Test]
    public void Correct()
    {
        var tokens = Tokenizer.ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(ExpectedTokens.Length + 1, Is.EqualTo(tokens.Length)); // Include End token
            Assert.That(tokens[^1].Kind, Is.EqualTo(TokenKind.End));
        });
        for (var i = 0; i < ExpectedTokens.Length; i++)
        {
            TestContext.WriteLine($"Expected: {ExpectedTokens[i]}, Found: {tokens[i]}");
            Assert.Multiple(() =>
            {
                Assert.That(ExpectedTokens[i].Kind, Is.EqualTo(tokens[i].Kind));
                Assert.That(ExpectedTokens[i].Segment.ToString(), Is.EqualTo(tokens[i].Segment.ToString()));
                Assert.That(ExpectedTokens[i].Offset, Is.EqualTo(tokens[i].Offset));
                if (CheckValue)
                    Assert.That(ExpectedTokens[i].Value, Is.EqualTo(tokens[i].Value));
            });
        }
    }
}