using System;
using System.Collections.Generic;
using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.Parsing
{
    public delegate bool SearchCriterionConstraint(SearchCriterion searchCriterion);

    public delegate ListCombinationKind CombinationKindTransform(Token key, ListCombinationKind combinationKind);

    public delegate IEnumerable<SearchCriterion> StrandedEnumProcessor(Expression expression);

    public class Parser(Tokenizer tokenizer)
    {
        private readonly Stack<int> _ruleStartIndex = new();
        private readonly List<SearchCriterion> _searchCriteria = [];
        private readonly List<Expression> _strandedEnums = [];
        private readonly List<Token> _tokens = [];
        private int _lookaheadPos;
        private Token? _lookaheadToken;
        private int _ruleEndPos = -1;

        /// <summary>
        ///     How to handle stranded enums (enums appearing without search filters (i.e. key=value)).
        ///     Set to Disabled to disallow any stranded enums
        /// </summary>
        public StrandedEnumPolicy EnumPolicy { get; set; } = StrandedEnumPolicy.Default;

        internal bool RequireCompleteStrandedEnum => EnumPolicy.HasFlag(StrandedEnumPolicy.RequireCompleteEnum);


        public SearchCriterionConstraint SearchCriterionConstraint { get; set; } = _ => true;

        /// <summary>
        ///     Changes whether the elements in a given list should be interpreted as an AND or an OR relation
        /// </summary>
        public CombinationKindTransform CombinationKindTransform { get; set; } =
            (_, combinationKind) => combinationKind;

        public StrandedEnumProcessor StrandedEnumProcessor { get; set; } = _ => [];

        public IReadOnlyList<SearchCriterion> SearchCriteria => _searchCriteria.AsReadOnly();
        public IReadOnlyList<Expression> StrandedEnums => _strandedEnums.AsReadOnly();

        internal Token Lookahead()
        {
            return _lookaheadToken ??= tokenizer.NextToken();
        }

        internal Token Consume()
        {
            var consumed = Lookahead();
            Advance();
            return consumed;
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

        internal bool ParseAtom(bool isStrandedValue, out AtomicValue value)
        {
            PushIndex();
            var lookahead = Lookahead();

            var success = !isStrandedValue && lookahead.TryCollapseKeyword(TokenKind.Enum, false)
                          || lookahead.Kind.IsValue();
            if (isStrandedValue && !lookahead.TryCollapseKeyword(TokenKind.Enum, RequireCompleteStrandedEnum))
                success = false;

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

        internal bool ParseExpression(bool isStrandedValue, out Expression expression)
        {
            PushIndex();

            var success = ParseAtom(isStrandedValue, out var startingValue);
            expression = startingValue;

            var combinationKind = ListCombinationKind.None;
            while (true)
            {
                if (expression is AtomicValue)
                {
                    if (!Match(TokenKind.Or, out var separator) && !Match(TokenKind.And, out separator))
                        break;
                    combinationKind = separator.Kind == TokenKind.Or
                        ? ListCombinationKind.Or
                        : ListCombinationKind.And;
                    expression = new ListValue([startingValue], combinationKind);
                }
                else if (combinationKind == ListCombinationKind.And && !Match(TokenKind.And, out _)
                         || combinationKind == ListCombinationKind.Or && !Match(TokenKind.Or, out _))
                    break;

                if (!ParseAtom(isStrandedValue, out var nextValue))
                {
                    success = false;
                    break;
                }

                ((ListValue)expression).Values.Add(nextValue);
            }

            expression.TokenRange = PopIndex();
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

            if (value is ListValue listValue)
            {
                listValue.CombinationKind = CombinationKindTransform(keyToken, listValue.CombinationKind);
            }

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
                if (ParseExpression(true, out var strandedExpression))
                {
                    _strandedEnums.Add(strandedExpression);

                    // Generate search criteria using this stranded enum
                    var generatedSearchCriteria = StrandedEnumProcessor(strandedExpression);
                    _searchCriteria.AddRange(generatedSearchCriteria);

                    // If stranded enums should not be included in plain text terms
                    // We mark them as included in criterion, so it will be skipped in GetPlainTextTerms()
                    if (!EnumPolicy.HasFlag(StrandedEnumPolicy.IncludeInPlainText))
                        MarkCriterionInclusion(strandedExpression.TokenRange, true);
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

        private bool ValidateStrandedEnum(Token lookahead)
        {
            return EnumPolicy.HasFlag(StrandedEnumPolicy.RequireCompleteEnum) &&
                   !lookahead.IsCompleteKeyword;
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
                if (token.IncludedInCriterion || token.Kind == TokenKind.End)
                {
                    if (plainTextConversionStartToken == null)
                        continue;

                    var start = plainTextConversionStartToken.Offset;
                    var end = token.Offset - 1;
                    var segment = tokenizer.Content.AsMemory().Slice(start, end - start + 1);
                    foreach (var plainTextToken in Tokenizer.TokenizeAsPlainTextTokens(segment, token.Offset))
                    {
                        yield return plainTextToken;
                    }

                    plainTextConversionStartToken = null;
                }
                else
                {
                    plainTextConversionStartToken ??= token;
                }
            }
        }
    }
}