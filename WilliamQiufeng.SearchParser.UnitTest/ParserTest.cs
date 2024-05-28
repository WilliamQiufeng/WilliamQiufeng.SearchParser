using WilliamQiufeng.SearchParser.Parsing;

namespace WilliamQiufeng.SearchParser.UnitTest;

// Empty
[TestFixture(new string[0], "", new object[0], new string[0])]
// Simple
[TestFixture(new[] { "a" }, "a>3 xyz", new object[]
    {
        new object[] { "a", TokenKind.MoreThan, 3, false }
    },
    new[] { "xyz" })]
// Criteria parsing with space, in-between two plain text terms
[TestFixture(new[] { "a" }, "abc a <= 3 xyz", new object[]
    {
        new object[] { "a", TokenKind.LessThanOrEqual, 3, false }
    },
    new[] { "abc", "xyz" })]
// Normal query
[TestFixture(new[] { "difficulty", "length", "lns" }, "sec d<=25 d>=20 ln>25% l<2m25s", new object[]
    {
        new object[] { "difficulty", TokenKind.LessThanOrEqual, 25, false },
        new object[] { "difficulty", TokenKind.MoreThanOrEqual, 20, false },
        new object[] { "lns", TokenKind.MoreThan, 25, false },
        new object?[] { "length", TokenKind.LessThan, null, false },
    },
    new[] { "sec" })]
// Strings, invert
[TestFixture(new[] { "title", "tag", "source" }, "t:\"except you\" !tag:\"> <\" is", new object[]
    {
        new object[] { "title", TokenKind.Contains, "except you", false },
        new object[] { "tag", TokenKind.Contains, "> <", true },
    },
    new[] { "is" })]
// Invalid criteria
[TestFixture(new[] { "title", "tag", "source" }, "t:! d>890 t:2 :3 |123 t! !3 t/3", new object[]
    {
        new object[] { "title", TokenKind.Contains, 2, false },
    },
    new[] { "t:!", "d>890", ":3", "|123", "t!", "!3", "t/3" })]
// Or patterns
[TestFixture(new[] { "title", "tag", "source" }, "t/tag: hi 123", new object[]
    {
        new object[] { "title", TokenKind.Contains, "hi", false },
        new object[] { "tag", TokenKind.Contains, "hi", false },
    },
    new[] { "123" })]
// Or patterns: duplicate keys are removed
[TestFixture(new[] { "title", "tag", "source" }, "t/tag/ta/t/s/s: hi 123", new object[]
    {
        new object[] { "title", TokenKind.Contains, "hi", false },
        new object[] { "tag", TokenKind.Contains, "hi", false },
        new object[] { "source", TokenKind.Contains, "hi", false },
    },
    new[] { "123" })]
public class ParserTest
{
    private readonly string _source;
    private readonly SearchCriterion[] _targetCriteria;
    private readonly string[] _targetPlainTextTerms;
    private readonly string[] _keys;

    public ParserTest(string[] keys, string source, object[] targetCriteriaConstructors, string[] targetPlainTextTerms)
    {
        _source = source;
        _targetPlainTextTerms = targetPlainTextTerms;
        _keys = keys;
        _targetCriteria = targetCriteriaConstructors.Cast<object?[]>().Select(p =>
        {
            var key = (string)p[0]!;
            return new SearchCriterion(new ArraySegment<Token>(),
                new Token(TokenKind.PlainText, key.AsMemory(), 0, key),
                new Token((TokenKind)p[1]!),
                new Token(TokenKind.Unknown, new ReadOnlyMemory<char>(), 0, p[2]), (bool)p[3]!);
        }).ToArray();
    }

    /// <summary>
    ///     Verifies that all criteria are parsed correctly and leaving the correct plain text terms.
    /// </summary>
    [Test]
    public void Correct()
    {
        var tokenizer = new Tokenizer(_source);
        foreach (var key in _keys)
        {
            tokenizer.KeywordTrie.Add(key, key);
        }

        var parser = new Parser(tokenizer.ToArray());
        parser.Parse();

        var terms = parser.GetPlainTextTerms(_source).ToArray();
        var criteria = parser.SearchCriteria;
        Assert.Multiple(() =>
        {
            Assert.That(terms, Has.Length.EqualTo(_targetPlainTextTerms.Length));
            Assert.That(criteria, Has.Count.EqualTo(_targetCriteria.Length));
        });
        for (var i = 0; i < terms.Length; i++)
        {
            var term = terms[i];
            Assert.That((string)term.Value!, Is.EqualTo(_targetPlainTextTerms[i]));
        }

        for (var i = 0; i < criteria.Count; i++)
        {
            var criterion = criteria[i];
            Assert.Multiple(() =>
            {
                Assert.That(criterion.Key.Value, Is.EqualTo(_targetCriteria[i].Key.Value));
                if (_targetCriteria[i].Value.Value != null)
                    Assert.That(criterion.Value.Value, Is.EqualTo(_targetCriteria[i].Value.Value));
                Assert.That(criterion.Operator.Value, Is.EqualTo(_targetCriteria[i].Operator.Value));
                Assert.That(criterion.Invert, Is.EqualTo(_targetCriteria[i].Invert));
            });
        }
    }

    /// <summary>
    ///     Nullifies all keys using <see cref="Parser.SearchCriterionConstraint"/>.
    ///     All search criteria should be marked as plain text.
    /// </summary>
    [Test]
    public void IgnoreAllKeys()
    {
        var tokenizer = new Tokenizer(_source);
        foreach (var key in _keys)
        {
            tokenizer.KeywordTrie.Add(key, key);
        }

        var parser = new Parser(tokenizer.ToArray());
        parser.SearchCriterionConstraint = _ => false;
        parser.Parse();

        var terms = parser.GetPlainTextTerms(_source).ToArray();
        var criteria = parser.SearchCriteria;
        var targetTerms = Tokenizer.TokenizeAsPlainTextTokens(_source.AsMemory()).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(terms, Has.Length.EqualTo(targetTerms.Length));
            Assert.That(criteria, Is.Empty);
        });
        for (var i = 0; i < terms.Length; i++)
        {
            var term = terms[i];
            Assert.That((string)term.Value!, Is.EqualTo(targetTerms[i].Value));
        }
    }
}