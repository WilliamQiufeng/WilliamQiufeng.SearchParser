namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class EmptyState : ITokenizerState
    {
        public static readonly EmptyState State = new();

        public ITokenizerState Process(Tokenizer tokenizer)
        {
            var lookahead = tokenizer.Lookahead();

            switch (lookahead)
            {
                // End of query
                case '\0':
                    return EndState.State;
                // Discard all whitespaces
                case ' ':
                    tokenizer.Advance();
                    tokenizer.DiscardBuffer();
                    return this;
                case '=':
                {
                    tokenizer.Advance();
                    if (tokenizer.Lookahead() == '=')
                        tokenizer.Advance();
                    tokenizer.EmitToken(TokenKind.Equal);
                    return this;
                }
                case '>':
                {
                    tokenizer.Advance();
                    if (tokenizer.Lookahead() == '=')
                    {
                        tokenizer.Advance();
                        tokenizer.EmitToken(TokenKind.MoreThanOrEqual);
                    }
                    else
                    {
                        tokenizer.EmitToken(TokenKind.MoreThan);
                    }

                    return this;
                }
                case '<':
                {
                    tokenizer.Advance();
                    if (tokenizer.Lookahead() == '=')
                    {
                        tokenizer.Advance();
                        tokenizer.EmitToken(TokenKind.LessThanOrEqual);
                    }
                    else
                    {
                        tokenizer.EmitToken(TokenKind.LessThan);
                    }

                    return this;
                }
                case '!':
                {
                    tokenizer.Advance();
                    if (tokenizer.Lookahead() == '=')
                    {
                        tokenizer.Advance();
                        tokenizer.EmitToken(TokenKind.NotEqual);
                    }
                    else
                    {
                        tokenizer.EmitToken(TokenKind.Not);
                    }

                    return this;
                }
                case '/':
                case '|':
                    tokenizer.Advance();
                    tokenizer.EmitToken(TokenKind.Or);
                    return this;
                case ',':
                    tokenizer.Advance();
                    tokenizer.EmitToken(TokenKind.And);
                    return this;
                case ':':
                    tokenizer.Advance();
                    tokenizer.EmitToken(TokenKind.Contains);
                    return this;
                case '.':
                    tokenizer.Advance();
                    return new RealState();
                case >= '0' and <= '9':
                    return new IntegerState();
                case '"' or '\'':
                    return new StringState();
            }

            if (tokenizer.KeywordTrie.TryNext(lookahead, out _))
                return new KeyState(tokenizer.KeywordTrie);

            return PlainTextState.State;
        }
    }
}