using System;
using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.Parsing
{
    public class Rule
    {
        public Rule(ArraySegment<Token> tokens)
        {
            Tokens = tokens;
        }

        protected ArraySegment<Token> Tokens { get; set; }
    }
}