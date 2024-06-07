using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.Parsing
{
    public class SearchCriterion(
        Token key,
        Token @operator,
        ListValue values,
        bool invert,
        TokenRange tokenRange = new())
        : Nonterminal(tokenRange)
    {
        public SearchCriterion(object? keyContent, TokenKind operatorKind,
            ListValue values, bool invert)
            : this(new Token(TokenKind.Key, content: keyContent), new Token(operatorKind), values,
                invert)
        {
        }

        public Token Key { get; } = key;
        public Token Operator { get; } = @operator;

        public ListValue Values { get; } = values;
        public bool Invert { get; } = invert;
    }
}