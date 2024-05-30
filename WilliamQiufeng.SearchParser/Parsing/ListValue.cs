using System;
using System.Collections.Generic;
using System.Linq;
using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.Parsing;

public class ListValue(
    List<AtomicValue> values,
    ListCombinationKind combinationKind = ListCombinationKind.None,
    TokenRange tokenRange = default)
    : Expression(tokenRange), IEquatable<ListValue>
{
    public ListValue(TokenKind elementKind, IEnumerable<object?> values,
        ListCombinationKind combinationKind = ListCombinationKind.None)
        : this(values.Select(v => new AtomicValue(elementKind, v)).ToList(), combinationKind)
    {
    }

    public List<AtomicValue> Values { get; } = values;
    public ListCombinationKind CombinationKind { get; internal set; } = combinationKind;

    public override object Value => Values;

    public bool Equals(ListValue? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Values.SequenceEqual(other.Values) && CombinationKind == other.CombinationKind;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ListValue)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Values, (int)CombinationKind);
    }
}