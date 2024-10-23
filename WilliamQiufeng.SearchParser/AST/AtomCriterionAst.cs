using System;
using WilliamQiufeng.SearchParser.Parsing;
using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.AST;

public record AtomCriterionAst(
    Token Key,
    Token Operator,
    AtomicValue Value) : Ast
{
    public Token Key { get; } = Key;
    public Token Operator { get; } = Operator;
    public AtomicValue Value { get; } = Value;

    public override bool Evaluate<T>(T obj, Func<AtomCriterionAst, T, bool> predicate)
    {
        return predicate(this, obj);
    }
}