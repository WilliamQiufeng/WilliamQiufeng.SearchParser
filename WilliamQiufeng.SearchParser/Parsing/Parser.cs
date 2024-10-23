using System;
using System.Collections.Generic;
using System.Linq;
using WilliamQiufeng.SearchParser.AST;
using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.Parsing
{
    public delegate bool SearchCriterionConstraint(SearchCriterion searchCriterion);

    public delegate ListCombinationKind CombinationKindTransform(Token key, ListCombinationKind combinationKind);

    public delegate IEnumerable<SearchCriterion> SingletonEnumProcessor(ListValue listValue);

    public class Parser(Tokenizer tokenizer)
    {
        private readonly Stack<int> _ruleStartIndex = new();
        private readonly List<SearchCriterion> _searchCriteria = [];
        private readonly List<Expression> _singletonEnums = [];
        private readonly List<Token> _tokens = [];
        private int _lookaheadPos;
        private Token? _lookaheadToken;
        private int _ruleEndPos = -1;

        /// <summary>
        ///     How to handle singleton enums (enums appearing without search filters (i.e. key=value)).
        ///     Set to Disabled to disallow any singleton enums
        /// </summary>
        public SingletonEnumPolicy EnumPolicy { get; set; } = SingletonEnumPolicy.Default;

        internal bool RequireCompleteSingletonEnum => EnumPolicy.HasFlag(SingletonEnumPolicy.RequireCompleteEnum);


        public SearchCriterionConstraint SearchCriterionConstraint { get; set; } = _ => true;

        /// <summary>
        ///     Changes whether the elements in a given list should be interpreted as an AND or an OR relation
        /// </summary>
        public CombinationKindTransform CombinationKindTransform { get; set; } =
            (_, combinationKind) => combinationKind;

        public SingletonEnumProcessor SingletonEnumProcessor { get; set; } = _ => [];

        public IReadOnlyList<SearchCriterion> SearchCriteria => _searchCriteria.AsReadOnly();
        public IReadOnlyList<Expression> SingletonEnums => _singletonEnums.AsReadOnly();

        internal Token Lookahead()
        {
            return _lookaheadToken ??= tokenizer.NextToken();
        }

        private void Advance()
        {
            _ruleEndPos = _lookaheadPos;
            _tokens.Add(_lookaheadToken ?? Lookahead());
            _lookaheadToken = null;
            if (_lookaheadPos < _tokens.Count)
                _lookaheadPos++;
        }

        internal bool Match(TokenKind tokenKind, out Token consumed)
        {
            consumed = Lookahead();
            if (consumed.Kind != tokenKind)
                return false;

            Advance();
            return true;
        }

        internal bool MatchKeyword(TokenKind tokenKind, bool requireCompleteKeyword, out Token consumed)
        {
            consumed = Lookahead();
            if (!consumed.TryCollapseKeyword(tokenKind, requireCompleteKeyword))
                return false;

            Advance();
            return true;
        }

        internal void PushIndex()
        {
            _ruleStartIndex.Push(_lookaheadPos);
        }

        internal TokenRange PopIndex()
        {
            var startIndex = _ruleStartIndex.Pop();
            var endIndex = _ruleEndPos;
            return new TokenRange(startIndex, endIndex);
        }

        internal bool ParseAtom(bool isSingletonValue, out AtomicValue value)
        {
            PushIndex();
            var lookahead = Lookahead();

            var success =
                !isSingletonValue && (lookahead.TryCollapseKeyword(TokenKind.Enum, false) ||
                                      lookahead.TryCollapseKeyword(TokenKind.PlainText, false))
                || lookahead.Kind.IsValue();
            if (isSingletonValue)
                success = lookahead.TryCollapseKeyword(TokenKind.Enum, RequireCompleteSingletonEnum);

            if (success)
            {
                Advance();
                var range = PopIndex();
                value = new AtomicValue(lookahead, range);
                return true;
            }

            value = AtomicValue.Null;
            _ = PopIndex();
            return false;
        }

        internal bool ParseExpression(bool isSingletonValue, out ListValue listValue)
        {
            PushIndex();

            var success = ParseAtom(isSingletonValue, out var startingValue);

            var combinationKind = ListCombinationKind.None;
            listValue = new ListValue([startingValue], combinationKind);

            while (true)
            {
                if (listValue.Count == 1)
                {
                    if (!Match(TokenKind.Or, out var separator) && !Match(TokenKind.And, out separator))
                        break;
                    combinationKind = separator.Kind == TokenKind.Or
                        ? ListCombinationKind.Or
                        : ListCombinationKind.And;
                }
                else if (combinationKind == ListCombinationKind.And && !Match(TokenKind.And, out _)
                         || combinationKind == ListCombinationKind.Or && !Match(TokenKind.Or, out _))
                {
                    // Two cases here:
                    // 1. Not success: Inconsistent combination kind (a,b/c or a/b,c etc.), lookahead must be either 'And' or 'Or'
                    // 2. Success: End of list, lookahead is neither 'And' nor 'Or'.
                    success = Lookahead().Kind is not (TokenKind.And or TokenKind.Or);
                    break;
                }

                if (!ParseAtom(isSingletonValue, out var nextValue))
                {
                    success = false;
                    break;
                }

                listValue.Add(nextValue);
            }

            listValue.TokenRange = PopIndex();
            return success;
        }

        internal bool ParseSearchCriterion(out TokenRange range)
        {
            PushIndex();
            var invert = Match(TokenKind.Not, out _);
            var keyTokens = new List<Token>();
            var keys = new HashSet<object?>();
            if (!MatchKeyword(TokenKind.Key, false, out var keyToken))
            {
                range = PopIndex();
                return false;
            }

            keyTokens.Add(keyToken);
            keys.Add(keyToken.Value);

            while (Match(TokenKind.Or, out _))
            {
                if (!MatchKeyword(TokenKind.Key, false, out var trailingKeyToken))
                {
                    range = PopIndex();
                    return false;
                }

                if (keys.Add(trailingKeyToken.Value))
                    keyTokens.Add(trailingKeyToken);
            }

            var operatorToken = Lookahead();
            if (operatorToken.Kind.IsRelational())
            {
                Advance();
            }
            else
            {
                range = PopIndex();
                return false;
            }

            if (!ParseExpression(false, out var value))
            {
                range = PopIndex();
                return false;
            }

            value.CombinationKind = CombinationKindTransform(keyToken, value.CombinationKind);

            range = PopIndex();

            var criteria = new List<SearchCriterion>();
            foreach (var key in keyTokens)
            {
                var searchCriterion = new SearchCriterion(key, operatorToken, value, invert, range);
                if (!SearchCriterionConstraint(searchCriterion))
                    return false;
                criteria.Add(searchCriterion);
            }

            _searchCriteria.AddRange(criteria);
            return true;
        }

        public void Parse()
        {
            while (Lookahead() is var lookahead && lookahead.Kind != TokenKind.End)
            {
                if (ParseExpression(true, out var singletonExpression))
                {
                    _singletonEnums.Add(singletonExpression);

                    // Generate search criteria using this singleton enum
                    // We perform rollback if any of the criteria generated does not match the constraint function
                    var generatedSearchCriteria = SingletonEnumProcessor(singletonExpression).ToList();
                    if (generatedSearchCriteria.Any(c => !SearchCriterionConstraint(c))) continue;

                    _searchCriteria.AddRange(generatedSearchCriteria);

                    // If singleton enums should not be included in plain text terms
                    // We mark them as included in criterion, so it will be skipped in GetPlainTextTerms()
                    if (!EnumPolicy.HasFlag(SingletonEnumPolicy.IncludeInPlainText))
                        MarkCriterionInclusion(singletonExpression.TokenRange, true);
                }
                else if (ParseSearchCriterion(out var range))
                {
                    MarkCriterionInclusion(range, true);
                }
                else
                {
                    Advance();
                }
            }

            Advance();
        }

        public Ast GetAst()
        {
            var terms = GetPlainTextTerms().ToArray();
            var criteria = SearchCriteria;
            var rootCriteria = new ListCriterionAst(
                criteria.Select(x => x.Flatten()).Cast<Ast>()
                    .Concat(terms.Select(t =>
                        new AtomCriterionAst(
                            new Token(TokenKind.PlainText),
                            new Token(TokenKind.Contains),
                            new AtomicValue(t)))).ToList(),
                ListCombinationKind.And,
                false);
            return rootCriteria;
        }

        private void MarkCriterionInclusion(TokenRange range, bool success)
        {
            foreach (var index in range)
            {
                _tokens[index].IncludedInCriterion = success;
            }
        }

        public IEnumerable<Token> GetPlainTextTerms()
        {
            Token? plainTextConversionStartToken = null;
            foreach (var token in _tokens)
            {
                if (token.IncludedInCriterion || token.Kind is TokenKind.End or TokenKind.String)
                {
                    var isString = !token.IncludedInCriterion && token.Kind == TokenKind.String;
                    if (plainTextConversionStartToken == null)
                    {
                        if (isString)
                            yield return token;
                        continue;
                    }

                    var start = plainTextConversionStartToken.Offset;
                    var end = token.Offset - 1;
                    var segment = tokenizer.Content.AsMemory().Slice(start, end - start + 1);
                    foreach (var plainTextToken in Tokenizer.TokenizeAsPlainTextTokens(segment, token.Offset))
                    {
                        yield return plainTextToken;
                    }

                    plainTextConversionStartToken = null;

                    if (isString)
                        yield return token;
                }
                else
                {
                    plainTextConversionStartToken ??= token;
                }
            }
        }
    }
}