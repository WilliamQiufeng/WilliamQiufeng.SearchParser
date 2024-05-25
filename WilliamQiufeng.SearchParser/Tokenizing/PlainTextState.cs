namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class PlainTextState : ITokenizerState
    {
        public static readonly PlainTextState State = new PlainTextState();

        public ITokenizerState Process(Tokenizer tokenizer)
        {
            var lookahead = tokenizer.Lookahead();

            // We have reached a word boundary: emit a token
            if (lookahead == '\0' || lookahead == ' ')
            {
                tokenizer.EmitToken(TokenKind.PlainText);
                return EmptyState.State;
            }

            tokenizer.Consume();
            return this;
        }
    }
}