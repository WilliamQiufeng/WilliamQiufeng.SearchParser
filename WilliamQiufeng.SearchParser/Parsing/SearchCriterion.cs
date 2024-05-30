using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.Parsing
{
    public class SearchCriterion(TokenRange tokenRange, Token key, Token @operator, Expression value, bool invert)
        : Nonterminal(tokenRange)
    {
        public Token Key { get; } = key;
        public Token Operator { get; } = @operator;
        public Expression Value { get; } = value;
        public bool Invert { get; } = invert;
    }
}