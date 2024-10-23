using System;

namespace WilliamQiufeng.SearchParser.AST;

public abstract record Ast
{
    public abstract bool Evaluate<T>(T obj, Func<AtomCriterionAst, T, bool> func);
}