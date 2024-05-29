using System.Text;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class StringState : ITokenizerState
    {
        private readonly StringBuilder _stringBuilder = new();
        private char _quoteChar = '\0';

        public ITokenizerState Process(Tokenizer tokenizer)
        {
            var lookahead = tokenizer.Lookahead();

            if (lookahead == '\0')
                return PlainTextState.State;

            if (lookahead is '"' or '\'' && _quoteChar == '\0')
            {
                _quoteChar = lookahead;
                tokenizer.Advance();
                return this;
            }

            if (lookahead == _quoteChar)
            {
                tokenizer.Advance();
                tokenizer.EmitToken(TokenKind.String, _stringBuilder.ToString());
                return EmptyState.State;
            }

            _stringBuilder.Append(lookahead);
            tokenizer.Advance();
            return this;
        }
    }
}