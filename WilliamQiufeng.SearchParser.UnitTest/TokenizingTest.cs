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
public class TokenizingTest
{
    [SetUp]
    public void SetUp()
    {
        Tokenizer = new Tokenizer(Source);
    }

    public Token[] ExpectedTokens;
    public string Source;
    public Tokenizer Tokenizer;

    public TokenizingTest(string source, object[] expected)
    {
        Source = source;
        ExpectedTokens = expected.Cast<object[]>()
            .Select(e => new Token((TokenKind)e[0], ((string)e[1]).AsMemory(), (int)e[2], (string)e[3])).ToArray();
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