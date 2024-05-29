using System.Linq;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class KeyState : ITokenizerState
    {
        private Trie<object> _trie;

        public KeyState(Trie<object> trie)
        {
            _trie = trie;
        }

        public ITokenizerState Process(Tokenizer tokenizer)
        {
            var lookahead = tokenizer.Lookahead();

            // We have reached the end, or a relational operator is reached: emit the key token
            if (lookahead.IsKeyEnd())
            {
                // emit the key of top candidate
                // override content with the full candidate content
                tokenizer.EmitToken(TokenKind.Key, _trie.Candidates.First());

                return EmptyState.State;
            }

            // No candidates left: convert into plain text
            if (!_trie.TryNext(lookahead, out var nextTrie))
                return PlainTextState.State;

            // Possible candidates
            tokenizer.Consume();
            _trie = nextTrie;
            return this;
        }
    }
}