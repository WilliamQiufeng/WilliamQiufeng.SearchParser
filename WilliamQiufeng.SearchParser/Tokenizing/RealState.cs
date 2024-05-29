namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class RealState : ITokenizerState
    {
        public ITokenizerState Process(Tokenizer tokenizer)
        {
            var lookahead = tokenizer.Lookahead();

            if (lookahead is '\0' or ' ')
            {
                if (!TryEmit(tokenizer))
                    return PlainTextState.State;
                return EmptyState.State;
            }

            if (lookahead != '.' && lookahead is < '0' or > '9')
                return PlainTextState.State;

            tokenizer.Consume();
            return this;
        }

        private bool TryEmit(Tokenizer tokenizer)
        {
            if (!double.TryParse(tokenizer.BufferContent.ToString(), out var value)) return false;
            tokenizer.EmitToken(TokenKind.Real, value);
            return true;
        }
    }
}