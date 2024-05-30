using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.Parsing
{
    public class SearchCriterion(
        Token key,
        Token @operator,
        Expression value,
        bool invert,
        TokenRange tokenRange = new())
        : Nonterminal(tokenRange)
    {
        public SearchCriterion(object? keyContent, TokenKind operatorKind, Expression value, bool invert)
            : this(new Token(TokenKind.Key, content: keyContent), new Token(operatorKind), value, invert)
        {
        }

        public Token Key { get; } = key;
        public Token Operator { get; } = @operator;
        public Expression Value { get; } = value;
        public bool Invert { get; } = invert;
    }
}