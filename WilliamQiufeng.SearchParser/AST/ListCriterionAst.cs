using System;
using System.Collections.Generic;
using WilliamQiufeng.SearchParser.Parsing;

namespace WilliamQiufeng.SearchParser.AST;

public record ListCriterionAst(List<Ast> Elements, ListCombinationKind CombinationKind, bool Invert) : Ast
{
    public List<Ast> Elements { get; } = Elements;
    public ListCombinationKind CombinationKind { get; } = CombinationKind;
    public bool Invert { get; } = Invert;

    public override bool Evaluate<T>(T obj, Func<AtomCriterionAst, T, bool> predicate)
    {
        return CombinationKind switch
        {
            ListCombinationKind.And => Elements.TrueForAll(ast => ast.Evaluate(obj, predicate)),
            ListCombinationKind.Or => Elements.Exists(ast => ast.Evaluate(obj, predicate)),
            ListCombinationKind.None when Elements.Count == 1 => Elements[0].Evaluate(obj, predicate),
            ListCombinationKind.None => false,
            _ => throw new ArgumentOutOfRangeException()
        } ^ Invert;
    }
}