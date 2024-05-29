namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public enum TokenKind
    {
        Unknown,
        End,
        PlainText,
        Key,
        Enum,
        String,
        Integer,
        Real,
        TimeSpan,
        Percentage,
        Equal,
        NotEqual,
        LessThan,
        MoreThan,
        LessThanOrEqual,
        MoreThanOrEqual,
        Contains,
        Not,
        Or
    }
}