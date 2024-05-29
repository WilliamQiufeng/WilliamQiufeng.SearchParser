namespace WilliamQiufeng.SearchParser.Parsing
{
    public class Nonterminal
    {
        public Nonterminal(TokenRange tokenRange)
        {
            TokenRange = tokenRange;
        }

        protected TokenRange TokenRange { get; set; }
    }
}