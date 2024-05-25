using System;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public struct Token
    {
        public readonly TokenKind Kind;
        public readonly ReadOnlyMemory<char> Segment;
        public readonly int Offset;
        private bool _markedAsPlain;

        public Token(TokenKind kind, ReadOnlyMemory<char> content = default, int offset = 0)
        {
            Kind = kind;
            Segment = content;
            Offset = offset;
            _markedAsPlain = Kind == TokenKind.PlainText;
        }

        /// <summary>
        ///     It is marked to be converted to a plain text token in parser stage
        /// </summary>
        public bool MarkedAsPlain
        {
            get => _markedAsPlain;
            internal set => _markedAsPlain = value;
        }

        public override string ToString()
        {
            return $"[{Kind} {Segment.ToString()} #{Offset}]";
        }
    }
}