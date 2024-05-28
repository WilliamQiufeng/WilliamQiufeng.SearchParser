using System.Text.RegularExpressions;
using StringSplitOptions = System.StringSplitOptions;

namespace WilliamQiufeng.SearchParser.UnitTest;

[TestFixture("", new int[0])]
[TestFixture("a b", new[] { 0, 2 })]
[TestFixture("abc def", new[] { 0, 4 })]
[TestFixture("abc  def", new[] { 0, 5 })]
[TestFixture("abc   def", new[] { 0, 6 })]
[TestFixture("abc", new[] { 0 })]
[TestFixture("abc ", new[] { 0 })]
[TestFixture("abc  ", new[] { 0 })]
[TestFixture("a b c d", new[] { 0, 2, 4, 6 })]
[TestFixture("abc bcd cde def", new[] { 0, 4, 8, 12 })]
public partial class PlainTextTokenizingTest
{
    private readonly string _source;
    private string[] _targetTokenContents;
    private int[] _targetTokenOffsets;

    public PlainTextTokenizingTest(string source, int[] targetTokenOffsets)
    {
        _source = source;
        _targetTokenContents = string.IsNullOrEmpty(source)
            ? []
            : TrimMultipleSpace().Replace(source, " ").Split(' ',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        _targetTokenOffsets = targetTokenOffsets;
    }

    [Test]
    public void Correct()
    {
        var tokens = Tokenizer.TokenizeAsPlainTextTokens(_source.AsMemory()).ToArray();
        Assert.That(tokens, Has.Length.EqualTo(_targetTokenContents.Length));
        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            Assert.Multiple(() =>
            {
                Assert.That(token.Value, Is.EqualTo(_targetTokenContents[i]));
                Assert.That(token.Offset, Is.EqualTo(_targetTokenOffsets[i]));
            });
        }
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex TrimMultipleSpace();
}