namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class EmptyState : ITokenizerState
    {
        public static readonly EmptyState State = new EmptyState();

        public ITokenizerState Process(Tokenizer tokenizer)
        {
            var lookahead = tokenizer.Lookahead();

            // End of query
            if (lookahead == '\0')
                return EndState.State;
            // Discard all whitespaces
            if (lookahead == ' ')
            {
                tokenizer.Consume();
                tokenizer.DiscardBuffer();
                return this;
            }

            if (lookahead >= '0' && lookahead <= '9')
                return new IntegerState();

            if (lookahead == '"' || lookahead == '\'')
                return new StringState();

            if (tokenizer.KeywordTrie.TryNext(lookahead, out _))
                return new KeyState(tokenizer.KeywordTrie);

            return PlainTextState.State;
        }
    }
}