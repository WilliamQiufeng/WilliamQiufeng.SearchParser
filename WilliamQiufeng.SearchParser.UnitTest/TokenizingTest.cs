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
[TestFixture(new[] { "aaa", "aba", "aab", "abc" }, "a ab aa aab abc aac", new object[]
{
    new object[] { TokenKind.Key, "a", 0, "aaa" },
    new object[] { TokenKind.Key, "ab", 2, "aba" },
    new object[] { TokenKind.Key, "aa", 5, "aaa" },
    new object[] { TokenKind.Key, "aab", 8, "aab" },
    new object[] { TokenKind.Key, "abc", 12, "abc" },
    new object[] { TokenKind.PlainText, "aac", 16, "aac" }
})]
public class TokenizingTest
{
    public Token[] ExpectedTokens;
    public string Source;
    public Tokenizer Tokenizer;

    public TokenizingTest(string source, object[] expected)
    {
        Source = source;
        ExpectedTokens = expected.Cast<object[]>()
            .Select(e => new Token((TokenKind)e[0], ((string)e[1]).AsMemory(), (int)e[2], (string)e[3])).ToArray();
        Tokenizer = new Tokenizer(Source);
    }

    public TokenizingTest(string[] keys, string source, object[] expected) : this(source, expected)
    {
        foreach (var key in keys)
        {
            Tokenizer.KeywordTrie.Add(key);
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
                Assert.That(ExpectedTokens[i].Content, Is.EqualTo(tokens[i].Content));
            });
        }
    }
}