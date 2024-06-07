using WilliamQiufeng.SearchParser.Parsing;

namespace WilliamQiufeng.SearchParser.UnitTest;

#pragma warning disable CA1861
[TestFixture(new object[]
    {
        new object[] { "game", new[] { "quaver", "etterna", "osu", "malody" } },
        new object[] { "status", new[] { "ranked", "unranked", "unsubmitted" } },
    },
    "quaver/osu ranked",
    new object[]
    {
        new object[] { "game", TokenKind.Equal, new object[] { "quaver", "osu" }, false, ListCombinationKind.Or },
        new object[] { "status", TokenKind.Equal, "ranked", false },
    },
    new string[0])]
[TestFixture(new object[]
    {
        new object[] { "game", new[] { "quaver", "etterna", "osu", "malody" } },
        new object[] { "status", new[] { "ranked", "unranked", "unsubmitted" } },
    },
    "quave quaver/",
    new object[]
    {
    },
    new[] { "quave", "quaver/" })]
#pragma warning restore CA1861
public class SingletonEnumTest
{
    [SetUp]
    public void Setup()
    {
        _tokenizer = new Tokenizer(_source);

        foreach (var (@enum, key) in _enumKeyDictionary)
        {
            _tokenizer.KeywordTrie.Add(key, TokenKind.Key, key);
            _tokenizer.KeywordTrie.Add(@enum, TokenKind.Enum, @enum);
        }

        _parser = new Parser(_tokenizer);
        _parser.SingletonEnumProcessor = SingletonEnumProcessor;
    }

    private IEnumerable<SearchCriterion> SingletonEnumProcessor(ListValue expression)
    {
        var firstEnumContent = expression.FirstOrDefault()?.As<string>() ?? "";

        if (!_enumKeyDictionary.TryGetValue(firstEnumContent, out var key))
            yield break;
        yield return new SearchCriterion(key, TokenKind.Equal, expression, false);
    }

    private readonly string _source;
    private readonly SearchCriterion[] _targetCriteria;
    private readonly string[] _targetPlainTextTerms;
    private readonly Dictionary<string, string> _enumKeyDictionary = new();
    private Tokenizer _tokenizer;
    private Parser _parser;

    public SingletonEnumTest(object[] dict, string source, object[] targetCriteriaConstructors,
        string[] targetPlainTextTerms)
    {
        _source = source;
        _targetPlainTextTerms = targetPlainTextTerms;
        _targetCriteria = targetCriteriaConstructors.Cast<object?[]>().Select(p =>
        {
            var value = p[2] is object[] list
                ? new ListValue(TokenKind.Unknown, list,
                    (ListCombinationKind)p[4]!)
                : new ListValue(TokenKind.Unknown, [p[2]!]);

            return new SearchCriterion(p[0], (TokenKind)p[1]!, value, (bool)p[3]!);
        }).ToArray();
        foreach (var pair in dict.Cast<object[]>())
        {
            var key = (string)pair[0];
            var values = (string[])pair[1];
            foreach (var @enum in values)
            {
                _enumKeyDictionary.Add(@enum, key);
            }
        }
    }

    /// <summary>
    ///     Verifies that all criteria are parsed correctly and leaving the correct plain text terms.
    /// </summary>
    [Test]
    public void Correct()
    {
        _parser.Parse();
        var terms = _parser.GetPlainTextTerms().ToArray();
        var criteria = _parser.SearchCriteria;
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
                Assert.That(criterion.Values, Has.Count.EqualTo(_targetCriteria[i].Values.Count));
                for (var j = 0; j < criterion.Values.Count; j++)
                {
                    if (_targetCriteria[i].Values[j].Value != null)
                        Assert.That(criterion.Values[j].Value, Is.EqualTo(_targetCriteria[i].Values[j].Value));
                }

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
        _parser.SearchCriterionConstraint = _ => false;
        _parser.Parse();
        var terms = _parser.GetPlainTextTerms().ToArray();
        var criteria = _parser.SearchCriteria;
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