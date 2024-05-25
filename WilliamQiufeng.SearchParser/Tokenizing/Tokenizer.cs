using System;
using System.Collections;
using System.Collections.Generic;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class Tokenizer : IEnumerable<Token>
    {
        private readonly string _content;
        private readonly Queue<Token> _emittingTokens = new Queue<Token>();
        private int _currentTokenEndPos = -1;
        private int _currentTokenStartPos;
        private int _lookaheadPos;
        internal TokenizerState TokenizerState;

        public Tokenizer(string content)
        {
            _content = content;
            TokenizerState = new EmptyState();
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

        internal Token GenerateToken(TokenKind kind)
        {
            var length = _currentTokenEndPos - _currentTokenStartPos + 1;
            var token = new Token(kind,
                _content.AsMemory().Slice(_currentTokenStartPos, length),
                _currentTokenStartPos);
            _currentTokenStartPos = _lookaheadPos;
            _currentTokenEndPos = _currentTokenStartPos;
            return token;
        }

        internal void EmitToken(TokenKind kind)
        {
            EmitToken(GenerateToken(kind));
        }

        internal void EmitToken(Token token)
        {
            _emittingTokens.Enqueue(token);
        }

        private bool Next()
        {
            if (TokenizerState is EndState)
                return false;
            // TODO lookahead one char and decide the next state
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
    }
}