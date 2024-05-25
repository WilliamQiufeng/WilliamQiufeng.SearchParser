namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public interface ITokenizerState
    {
        public ITokenizerState Process(Tokenizer tokenizer);
    }
}