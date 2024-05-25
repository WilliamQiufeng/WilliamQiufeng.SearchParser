namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class IntegerState : ITokenizerState
    {
        private int _currentInteger;

        public ITokenizerState Process(Tokenizer tokenizer)
        {
            var lookahead = tokenizer.Lookahead();

            if (lookahead == '\0' || lookahead == ' ')
            {
                tokenizer.EmitToken(TokenKind.Integer, _currentInteger);
                return EmptyState.State;
            }

            if (lookahead == '%')
            {
                tokenizer.Consume();
                tokenizer.EmitToken(TokenKind.Percentage, _currentInteger);
                return EmptyState.State;
            }

            if (lookahead == '.')
            {
                tokenizer.Consume();
                return new RealState();
            }

            if (lookahead < '0' || lookahead > '9')
                return PlainTextState.State;

            tokenizer.Consume();
            _currentInteger = _currentInteger * 10 + lookahead - '0';
            return this;
        }
    }
}