using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.Parsing;

public class AtomicValue(TokenRange tokenRange, Token token) : Expression(tokenRange)
{
    public static readonly AtomicValue Null = new(new TokenRange(), new Token());
    public Token Token { get; } = token;
    public override object? Value => Token.Value;

    public T? As<T>() => (T?)Value;
}