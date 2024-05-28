using System;
using System.Collections.Generic;
using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.Parsing
{
    public delegate bool SearchCriterionConstraint(SearchCriterion searchCriterion);

    public class Parser
    {
        private readonly Stack<int> _ruleStartIndex = new Stack<int>();
        private readonly List<SearchCriterion> _searchCriteria = new List<SearchCriterion>();
        private readonly Token[] _tokens;
        private int _lookaheadPos;
        private int _ruleEndPos = -1;

        public Parser(Token[] tokens)
        {
            _tokens = tokens;
        }

        public SearchCriterionConstraint SearchCriterionConstraint { get; set; } = _ => true;
        public IReadOnlyList<SearchCriterion> SearchCriteria => _searchCriteria;

        internal Token Lookahead()
        {
            return _lookaheadPos < _tokens.Length ? _tokens[_lookaheadPos] : new Token();
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
            if (_lookaheadPos < _tokens.Length)
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

        internal ArraySegment<Token> PopIndex()
        {
            var startIndex = _ruleStartIndex.Pop();
            var endIndex = _ruleEndPos;
            return new ArraySegment<Token>(_tokens, startIndex, endIndex - startIndex + 1);
        }

        internal bool ParseValue(out Token? token)
        {
            PushIndex();
            var lookahead = Lookahead();

            switch (lookahead.Kind)
            {
                case TokenKind.Integer:
                case TokenKind.Real:
                case TokenKind.TimeSpan:
                case TokenKind.String:
                case TokenKind.PlainText:
                case TokenKind.Percentage:
                    token = lookahead;
                    Advance();
                    PopIndex();
                    return true;
                case TokenKind.Unknown:
                case TokenKind.End:
                case TokenKind.Key:
                case TokenKind.Equal:
                case TokenKind.NotEqual:
                case TokenKind.LessThan:
                case TokenKind.MoreThan:
                case TokenKind.LessThanOrEqual:
                case TokenKind.MoreThanOrEqual:
                case TokenKind.Contains:
                case TokenKind.Not:
                case TokenKind.Or:
                default:
                    token = null;
                    return Rewind(out _);
            }
        }

        internal bool ParseSearchCriterion(out ArraySegment<Token> range)
        {
            PushIndex();
            var invert = Match(TokenKind.Not, out _);
            var keyTokens = new List<Token>();
            var keys = new HashSet<object?>();
            if (!Match(TokenKind.Key, out var keyToken))
            {
                return Rewind(out range);
            }

            keyTokens.Add(keyToken);
            keys.Add(keyToken.Value);

            while (Match(TokenKind.Or, out _))
            {
                if (!Match(TokenKind.Key, out var trailingKeyToken))
                {
                    return Rewind(out range);
                }

                if (keys.Add(trailingKeyToken.Value))
                    keyTokens.Add(trailingKeyToken);
            }

            var operatorToken = Lookahead();
            switch (operatorToken.Kind)
            {
                case TokenKind.Equal:
                case TokenKind.NotEqual:
                case TokenKind.LessThan:
                case TokenKind.MoreThan:
                case TokenKind.LessThanOrEqual:
                case TokenKind.MoreThanOrEqual:
                case TokenKind.Contains:
                    Advance();
                    break;
                case TokenKind.Unknown:
                case TokenKind.End:
                case TokenKind.PlainText:
                case TokenKind.Key:
                case TokenKind.String:
                case TokenKind.Integer:
                case TokenKind.Real:
                case TokenKind.TimeSpan:
                case TokenKind.Percentage:
                case TokenKind.Or:
                case TokenKind.Not:
                default:
                    return Rewind(out range);
            }

            if (!ParseValue(out var value))
            {
                return Rewind(out range);
            }

            range = PopIndex();

            var criteria = new List<SearchCriterion>();
            foreach (var key in keyTokens)
            {
                var searchCriterion = new SearchCriterion(range, key, operatorToken, value ?? new Token(), invert);
                if (!SearchCriterionConstraint(searchCriterion))
                    return false;
                criteria.Add(searchCriterion);
            }

            _searchCriteria.AddRange(criteria);
            return true;
        }

        private bool Rewind(out ArraySegment<Token> range)
        {
            range = PopIndex();
            return false;
        }

        public void Parse()
        {
            while (Lookahead() is var lookahead && lookahead.Kind != TokenKind.End)
            {
                switch (lookahead.Kind)
                {
                    case TokenKind.PlainText:
                        Advance();
                        continue;
                    case TokenKind.Not:
                    case TokenKind.Key:
                    {
                        var success = ParseSearchCriterion(out var range);
                        for (var index = 0; index < range.Count; index++)
                        {
                            _tokens[index + range.Offset].MarkedAsPlain = !success;
                            _tokens[index + range.Offset].IncludedInCriterion = success;
                        }

                        break;
                    }
                    default:
                        Advance();
                        _tokens[_ruleEndPos].MarkedAsPlain = true;
                        break;
                }
            }
        }

        public IEnumerable<Token> GetPlainTextTerms(string source)
        {
            Token? plainTextConversionStartToken = null;
            foreach (var token in _tokens)
            {
                if (token.MarkedAsPlain)
                {
                    plainTextConversionStartToken ??= token;
                }
                else
                {
                    if (plainTextConversionStartToken == null)
                    {
                        if (token.Kind == TokenKind.PlainText && !token.IncludedInCriterion)
                            yield return token;
                        continue;
                    }

                    var start = plainTextConversionStartToken.Value.Offset;
                    var end = token.Offset - 1;
                    var segment = source.AsMemory().Slice(start, end - start + 1);
                    foreach (var plainTextToken in Tokenizer.TokenizeAsPlainTextTokens(segment, token.Offset))
                    {
                        yield return plainTextToken;
                    }

                    if (token.Kind == TokenKind.PlainText)
                        yield return token;

                    plainTextConversionStartToken = null;
                }
            }
        }
    }
}