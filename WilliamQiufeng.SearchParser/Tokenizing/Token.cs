using System;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class Token
    {
        public readonly TokenKind Kind;
        public readonly int Offset;
        public readonly ReadOnlyMemory<char> Segment;
        public readonly object? Value;
        public bool IncludedInCriterion;

        public Token()
        {
        }

        public Token(TokenKind kind, ReadOnlyMemory<char> segment = default, int offset = 0, object? content = default)
        {
            Kind = kind;
            Segment = segment;
            Offset = offset;
            Value = content;
            IncludedInCriterion = false;
        }

        public override string ToString()
        {
            return $"[{Kind} {Segment.ToString()} ({Value}) #{Offset}]";
        }
    }
}