using System;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public struct Token
    {
        public readonly TokenKind Kind;
        public readonly string Content;
        public readonly ReadOnlyMemory<char> Segment;
        public readonly int Offset;

        public Token(TokenKind kind, ReadOnlyMemory<char> segment = default, int offset = 0, string? content = default)
        {
            Kind = kind;
            Segment = segment;
            Offset = offset;
            MarkedAsPlain = Kind == TokenKind.PlainText;
            Content = content ?? segment.ToString();
        }

        /// <summary>
        ///     It is marked to be converted to a plain text token in parser stage
        /// </summary>
        public bool MarkedAsPlain { get; internal set; }

        public override string ToString()
        {
            return $"[{Kind} {Segment.ToString()} #{Offset}]";
        }
    }
}