using System;
using System.Collections.Generic;
using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.Parsing
{
    public delegate bool SearchCriterionConstraint(SearchCriterion searchCriterion);

    public delegate ListCombinationKind CombinationKindTransform(Token key, ListCombinationKind combinationKind);

    public delegate KeyEnumResolveMode ValueKeyEnumResolver(Token key);

    public delegate SearchCriterion? StrandedEnumProcessor(Token @enum);

    public class Parser(Tokenizer tokenizer)
    {
        private readonly Stack<int> _ruleStartIndex = new();
        private readonly List<SearchCriterion> _searchCriteria = [];
        private readonly List<Token> _tokens = [];
        private int _lookaheadPos;
        private Token? _lookaheadToken;
        private int _ruleEndPos = -1;

        /// <summary>
        ///     How to handle stranded enums (enums appearing without search filters (i.e. key=value)).
        ///     Set to Disabled to disallow any stranded enums
        /// </summary>
        public StrandedEnumPolicy StrandedEnumPolicy = StrandedEnumPolicy.Default;


        public SearchCriterionConstraint SearchCriterionConstraint { get; set; } = _ => true;

        /// <summary>
        ///     Changes whether the elements in a given list should be interpreted as an AND or an OR relation
        /// </summary>
        public CombinationKindTransform CombinationKindTransform { get; set; } =
            (_, combinationKind) => combinationKind;

        /// <summary>
        ///     Decides if the value could be interpreted as keys, enums or only plaintext.
        ///     For example, for tags you would not want to have enums resolved.
        /// </summary>
        public ValueKeyEnumResolver ValueKeyEnumResolver { get; set; } = _ => KeyEnumResolveMode.Enum;

        public StrandedEnumProcessor StrandedEnumProcessor { get; set; } = _ => null;

        public IReadOnlyList<SearchCriterion> SearchCriteria => _searchCriteria;
        public List<Token> StrandedEnums { get; } = [];

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

        internal bool ParseAtom(out AtomicValue value)
        {
            PushIndex();
            var lookahead = Lookahead();

            if (lookahead.Kind.IsValue())
            {
                Advance();
                var range = PopIndex();
                value = new AtomicValue(range, lookahead);
                return true;
            }

            value = AtomicValue.Null;
            _ = PopIndex();
            return false;
        }

        internal bool ParseExpression(out Expression expression, KeyEnumResolveMode resolveMode)
        {
            PushIndex();

            // We only want enums here, not keys
            tokenizer.KeyEnumResolveMode = resolveMode;
            var success = ParseAtom(out var startingValue);
            expression = startingValue;

            var combinationKind = ListCombinationKind.None;
            while (true)
            {
                // We expect either and/or token or a key token next.
                // We want to resolve keys if the next token is a key,
                // Setting resolve mode makes Tokenizer.Lookahead tokenize both keys and enums 
                tokenizer.KeyEnumResolveMode = KeyEnumResolveMode.Both;
                if (expression is AtomicValue)
                {
                    if (!Match(TokenKind.Or, out var separator) && !Match(TokenKind.And, out separator))
                        break;
                    combinationKind = separator.Kind == TokenKind.Or
                        ? ListCombinationKind.Or
                        : ListCombinationKind.And;
                    expression = new ListValue(new TokenRange(), [startingValue],
                        combinationKind);
                }
                else if (combinationKind == ListCombinationKind.And && !Match(TokenKind.And, out _)
                         || combinationKind == ListCombinationKind.Or && !Match(TokenKind.Or, out _))
                    break;

                tokenizer.KeyEnumResolveMode = resolveMode;
                if (!ParseAtom(out var nextValue))
                {
                    success = false;
                    break;
                }

                ((ListValue)expression).Values.Add(nextValue);
            }

            // Recover
            tokenizer.KeyEnumResolveMode = KeyEnumResolveMode.Both;
            expression.TokenRange = PopIndex();
            return success;
        }

        internal bool ParseSearchCriterion(out TokenRange range)
        {
            PushIndex();
            var invert = Match(TokenKind.Not, out _);
            var keyTokens = new List<Token>();
            var keys = new HashSet<object?>();
            if (!Match(TokenKind.Key, out var keyToken))
            {
                range = PopIndex();
                return false;
            }

            keyTokens.Add(keyToken);
            keys.Add(keyToken.Value);

            while (Match(TokenKind.Or, out _))
            {
                if (!Match(TokenKind.Key, out var trailingKeyToken))
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

            var resolveMode = ValueKeyEnumResolver(keyToken);
            if (!ParseExpression(out var value, resolveMode))
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
                var searchCriterion = new SearchCriterion(range, key, operatorToken, value, invert);
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
                switch (lookahead.Kind)
                {
                    case TokenKind.Not:
                    case TokenKind.Key:
                    {
                        var success = ParseSearchCriterion(out var range);
                        MarkCriterionInclusion(range, success);
                        break;
                    }
                    case TokenKind.Enum when StrandedEnumPolicy != StrandedEnumPolicy.Disabled:
                        // Skip if the enum is not fully specified when it should be
                        if (StrandedEnumPolicy.HasFlag(StrandedEnumPolicy.RequireCompleteEnum) &&
                            !lookahead.IsCompleteEnum)
                        {
                            Advance();
                            break;
                        }

                        // Generate a search criterion using this stranded enum
                        var generatedSearchCriterion = StrandedEnumProcessor(lookahead);

                        // Null means the developer doesn't want to generate any criterion for this enum
                        if (generatedSearchCriterion != null)
                            _searchCriteria.Add(generatedSearchCriterion);

                        StrandedEnums.Add(lookahead);

                        // If stranded enums should not be included in plain text terms
                        // We mark them as included in criterion, so it will be skipped in GetPlainTextTerms()
                        if (!StrandedEnumPolicy.HasFlag(StrandedEnumPolicy.IncludeInPlainText))
                            lookahead.IncludedInCriterion = true;

                        Advance();
                        break;
                    default:
                        Advance();
                        break;
                }
            }

            Advance();
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