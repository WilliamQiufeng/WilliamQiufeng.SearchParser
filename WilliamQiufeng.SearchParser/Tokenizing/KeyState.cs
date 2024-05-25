using System.Linq;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class KeyState : ITokenizerState
    {
        private Trie _trie;

        public KeyState(Trie trie)
        {
            _trie = trie;
        }

        public ITokenizerState Process(Tokenizer tokenizer)
        {
            var lookahead = tokenizer.Lookahead();

            // We have reached the end: emit a token
            if (lookahead == '\0' || lookahead == ' ')
            {
                if (_trie.Candidates.Count > 0)
                {
                    // emit the key of top candidate
                    // override content with the full candidate content
                    tokenizer.EmitToken(TokenKind.Key, _trie.Candidates.First());
                }
                else
                {
                    // No candidate found: convert into plain text
                    tokenizer.EmitToken(TokenKind.PlainText);
                }

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