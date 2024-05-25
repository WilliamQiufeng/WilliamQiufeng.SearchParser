namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public enum TokenKind
    {
        Unknown,
        End,
        PlainText,
        Key,
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
        Not
    }
}