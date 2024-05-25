using System;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public struct Token
    {
        public readonly TokenKind Kind;
        public readonly object? Value;
        public readonly ReadOnlyMemory<char> Segment;
        public readonly int Offset;

        public Token(TokenKind kind, ReadOnlyMemory<char> segment = default, int offset = 0, object? content = default)
        {
            Kind = kind;
            Segment = segment;
            Offset = offset;
            MarkedAsPlain = Kind == TokenKind.PlainText;
            Value = content;
        }

        /// <summary>
        ///     It is marked to be converted to a plain text token in parser stage
        /// </summary>
        public bool MarkedAsPlain { get; internal set; }

        public override string ToString()
        {
            return $"[{Kind} {Segment.ToString()} ({Value}) #{Offset}]";
        }
    }
}