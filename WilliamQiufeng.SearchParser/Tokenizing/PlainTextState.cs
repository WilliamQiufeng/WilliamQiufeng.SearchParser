namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class PlainTextState : ITokenizerState
    {
        public static readonly PlainTextState State = new();

        public ITokenizerState Process(Tokenizer tokenizer)
        {
            var lookahead = tokenizer.Lookahead();

            // We have reached a word boundary: emit a token
            if (lookahead.IsWordBoundary() || lookahead.IsListDelimiter())
            {
                tokenizer.EmitToken(TokenKind.PlainText, tokenizer.BufferContent.ToString());
                return EmptyState.State;
            }

            tokenizer.Advance();
            return this;
        }
    }
}