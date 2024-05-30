namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public static class TokenHelper
    {
        public static bool IsLookaheadOperator(this char c)
        {
            return c is '=' or '>' or '<' or '!' or ':';
        }

        public static bool IsOr(this char c)
        {
            return c is '/' or '|';
        }

        public static bool IsListDelimiter(this char c)
        {
            return c.IsOr() || c == ',';
        }

        public static bool IsKeyEnd(this char c)
        {
            return c.IsWordBoundary() || c.IsLookaheadOperator() || c.IsOr();
        }

        public static bool IsWordBoundary(this char c)
        {
            return c is '\0' or ' ';
        }

        public static bool IsEnumEnd(this char c)
        {
            return c.IsWordBoundary() || c == ',' || c.IsOr();
        }

        public static bool IsRelational(this TokenKind tokenKind)
        {
            return tokenKind is TokenKind.Contains
                or TokenKind.Equal or TokenKind.NotEqual
                or TokenKind.MoreThan or TokenKind.MoreThanOrEqual
                or TokenKind.LessThan or TokenKind.LessThanOrEqual;
        }

        public static bool IsValue(this TokenKind tokenKind)
        {
            return tokenKind is TokenKind.Integer
                or TokenKind.Real
                or TokenKind.TimeSpan
                or TokenKind.String
                or TokenKind.PlainText
                or TokenKind.Percentage
                or TokenKind.Enum;
        }
    }
}