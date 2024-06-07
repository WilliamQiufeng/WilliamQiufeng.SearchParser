using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.Parsing;

public class ListValue(
    List<AtomicValue> values,
    ListCombinationKind combinationKind = ListCombinationKind.None,
    TokenRange tokenRange = default)
    : Expression(tokenRange), IEquatable<ListValue>, IList<AtomicValue>
{
    public ListValue(TokenKind elementKind, IEnumerable<object?> values,
        ListCombinationKind combinationKind = ListCombinationKind.None)
        : this(values.Select(v => new AtomicValue(elementKind, v)).ToList(), combinationKind)
    {
    }

    public ListCombinationKind CombinationKind { get; internal set; } = combinationKind;

    public override object Value => values;

    public bool Equals(ListValue? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return this.SequenceEqual(other) && CombinationKind == other.CombinationKind;
    }

    public IEnumerator<AtomicValue> GetEnumerator()
    {
        return values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(AtomicValue item)
    {
        values.Add(item);
    }

    public void Clear()
    {
        values.Clear();
    }

    public bool Contains(AtomicValue item)
    {
        return values.Contains(item);
    }

    public void CopyTo(AtomicValue[] array, int arrayIndex)
    {
        values.CopyTo(array, arrayIndex);
    }

    public bool Remove(AtomicValue item)
    {
        return values.Remove(item);
    }

    public int Count => values.Count;
    public bool IsReadOnly => false;

    public int IndexOf(AtomicValue item)
    {
        return values.IndexOf(item);
    }

    public void Insert(int index, AtomicValue item)
    {
        values.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        values.RemoveAt(index);
    }

    public AtomicValue this[int index]
    {
        get => values[index];
        set => values[index] = value;
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
        return HashCode.Combine(base.GetHashCode(), values, (int)CombinationKind);
    }
}