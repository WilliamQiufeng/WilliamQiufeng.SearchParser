using System;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class Token
    {
        public readonly TokenKind Kind;
        public readonly int Offset;
        public readonly ReadOnlyMemory<char> Segment;
        public readonly object? Value;

        /// <summary>
        ///     Whether the token is a part of a search criterion
        /// </summary>
        public bool IncludedInCriterion;

        /// <summary>
        ///     If <see cref="TokenKind"/> is <see cref="TokenKind.Enum"/>, whether the enum is specified fully.
        ///     For example, this will be marked true if there is an enum "tag" and the content of this token is "tag".
        ///     If the content is "t" or "ta" instead, it will be false.
        /// </summary>
        public bool IsCompleteEnum;

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