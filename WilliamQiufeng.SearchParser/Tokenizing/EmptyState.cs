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

            return tokenizer.KeyEnumResolveMode switch
            {
                KeyEnumResolveMode.Key or KeyEnumResolveMode.Both when tokenizer.KeywordTrie.TryNext(lookahead, out _)
                    => new KeyState(tokenizer.KeywordTrie),
                KeyEnumResolveMode.Enum or KeyEnumResolveMode.Both when tokenizer.EnumTrie.TryNext(lookahead, out _)
                    => new EnumState(tokenizer.EnumTrie),
                _ => PlainTextState.State
            };
        }
    }
}