using System;
using System.Collections;
using System.Collections.Generic;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class Tokenizer : IEnumerable<Token>
    {
        private readonly string _content;
        private readonly Queue<Token> _emittingTokens = new Queue<Token>();
        private ITokenizerState _currentState;
        private int _currentTokenEndPos = -1;
        private int _currentTokenStartPos;
        private int _lookaheadPos;

        public Tokenizer(string content)
        {
            _content = content;
            _currentState = EmptyState.State;
        }

        public Trie<object> KeywordTrie { get; } = new Trie<object>();

        internal ReadOnlyMemory<char> BufferContent
        {
            get
            {
                var length = _currentTokenEndPos - _currentTokenStartPos + 1;
                var segment = _currentTokenStartPos < _content.Length
                    ? _content.AsMemory().Slice(_currentTokenStartPos, length)
                    : new ReadOnlyMemory<char>();
                return segment;
            }
        }

        public IEnumerator<Token> GetEnumerator()
        {
            Token nextToken;
            do
            {
                nextToken = NextToken();
                yield return nextToken;
            } while (nextToken.Kind != TokenKind.End);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal char Lookahead()
        {
            return _lookaheadPos < _content.Length ? _content[_lookaheadPos] : '\0';
        }

        internal char Consume()
        {
            var consumed = Lookahead();
            _currentTokenEndPos = _lookaheadPos;
            if (_lookaheadPos < _content.Length)
                _lookaheadPos++;
            return consumed;
        }

        internal Token GenerateToken(TokenKind kind, object? content = default)
        {
            var token = new Token(kind, BufferContent, _currentTokenStartPos, content);
            DiscardBuffer();
            return token;
        }

        internal void DiscardBuffer()
        {
            _currentTokenStartPos = _lookaheadPos;
            _currentTokenEndPos = _currentTokenStartPos;
        }

        internal void EmitToken(TokenKind kind, object? content = default)
        {
            EmitToken(GenerateToken(kind, content));
        }

        internal void EmitToken(Token token)
        {
            _emittingTokens.Enqueue(token);
        }

        private bool Next()
        {
            if (_currentState is EndState)
                return false;
            _currentState = _currentState.Process(this);
            return true;
        }

        /// <summary>
        ///     Continuously call <see cref="Next"/> until a token is emitted in the queue
        /// </summary>
        /// <returns></returns>
        private Token NextToken()
        {
            do
            {
                if (_emittingTokens.TryDequeue(out var nextToken))
                    return nextToken;
            } while (Next());

            return GenerateToken(TokenKind.End);
        }

        public static IEnumerable<Token> TokenizeAsPlainTextTokens(ReadOnlyMemory<char> view, int offset = 0)
        {
            var sliceStart = 0;
            var sliceEnd = -1;

            while (sliceEnd < view.Length)
            {
                if (sliceEnd == view.Length - 1 || view.Span[sliceEnd + 1] == ' ')
                {
                    var length = sliceEnd - sliceStart + 1;
                    if (length > 0)
                    {
                        var segment = view.Slice(sliceStart, length);
                        yield return new Token(TokenKind.PlainText, segment, sliceStart + offset, segment.ToString());
                    }

                    sliceStart = sliceEnd + 2;
                }

                sliceEnd++;
            }
        }
    }
}