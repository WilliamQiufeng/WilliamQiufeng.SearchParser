using System.Text;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class StringState : ITokenizerState
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private char _quoteChar = '\0';

        public ITokenizerState Process(Tokenizer tokenizer)
        {
            var lookahead = tokenizer.Lookahead();

            if (lookahead == '\0')
                return PlainTextState.State;

            if ((lookahead == '"' || lookahead == '\'') && _quoteChar == '\0')
            {
                _quoteChar = lookahead;
                tokenizer.Consume();
                return this;
            }

            if (lookahead == _quoteChar)
            {
                tokenizer.Consume();
                tokenizer.EmitToken(TokenKind.String, _stringBuilder.ToString());
                return EmptyState.State;
            }

            _stringBuilder.Append(lookahead);
            tokenizer.Consume();
            return this;
        }
    }
}