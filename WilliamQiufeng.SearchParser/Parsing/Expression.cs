using System;

namespace WilliamQiufeng.SearchParser.Parsing
{
    public abstract class Expression(TokenRange tokenRange) : Nonterminal(tokenRange), IEquatable<Expression>
    {
        public abstract object? Value { get; }

        public bool Equals(Expression? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Value, other.Value);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Expression)obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }
}