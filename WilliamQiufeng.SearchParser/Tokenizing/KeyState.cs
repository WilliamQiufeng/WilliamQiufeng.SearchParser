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
                tokenizer.EmitToken(TokenKind.Key, _trie.TerminalCandidate ?? _trie.Candidates.First());

                return EmptyState.State;
            }

            // No candidates left: convert into plain text
            if (!_trie.TryNext(lookahead, out var nextTrie))
            {
                // When we allow both key and enum to be tokenized, we do fallback on each other
                if (tokenizer.KeyEnumResolveMode is KeyEnumResolveMode.Both
                    && tokenizer.EnumTrie.TryNext(tokenizer.BufferContent.Span, out var subTrie))
                {
                    tokenizer.Advance();
                    return new EnumState(subTrie);
                }

                return PlainTextState.State;
            }

            // Possible candidates
            tokenizer.Advance();
            _trie = nextTrie;
            return this;
        }
    }
}