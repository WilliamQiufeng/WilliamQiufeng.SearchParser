namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public enum TokenKind
    {
        Unknown,
        End,
        PlainText,
        Key,
        String,
        Number,
        TimeSpan,
        Equal,
        NotEqual,
        LessThan,
        MoreThan,
        LessThanOrEqual,
        MoreThanOrEqual,
        Not
    }
}