namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class IntegerState : ITokenizerState
    {
        private int _currentInteger;

        public ITokenizerState Process(Tokenizer tokenizer)
        {
            var lookahead = tokenizer.Lookahead();

            switch (lookahead)
            {
                case '\0':
                case ' ':
                    tokenizer.EmitToken(TokenKind.Integer, _currentInteger);
                    return EmptyState.State;
                case '%':
                    tokenizer.Consume();
                    tokenizer.EmitToken(TokenKind.Percentage, _currentInteger);
                    return EmptyState.State;
                case '.':
                    tokenizer.Consume();
                    return new RealState();
            }

            if (TimeSpanState.Trie.TryNext(lookahead, out _) || lookahead == ':')
            {
                return new TimeSpanState(_currentInteger);
            }

            if (lookahead is < '0' or > '9')
                return PlainTextState.State;

            tokenizer.Consume();
            _currentInteger = _currentInteger * 10 + lookahead - '0';
            return this;
        }
    }
}