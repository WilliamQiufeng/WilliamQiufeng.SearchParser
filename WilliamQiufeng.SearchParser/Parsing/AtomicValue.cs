using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.Parsing;

public class AtomicValue(Token token, TokenRange tokenRange = default) : Expression(tokenRange)
{
    public static readonly AtomicValue Null = new(new Token());

    public AtomicValue(TokenKind kind, object? value) : this(new Token(kind, content: value), new TokenRange())
    {
    }

    public Token Token { get; } = token;
    public override object? Value => Token.Value;

    public T? As<T>() => (T?)Value;
}