namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class EndState : ITokenizerState
    {
        public static readonly EndState State = new EndState();

        public ITokenizerState Process(Tokenizer tokenizer)
        {
            throw new System.NotImplementedException();
        }
    }
}