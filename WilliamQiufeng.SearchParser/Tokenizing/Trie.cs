using System.Collections.Generic;
using System.Linq;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    using TrieCandidate = (TokenKind TokenKind, object Value);

    public class Trie
    {
        private readonly KeywordCandidates _candidateKeys = new();
        private readonly Dictionary<char, Trie> _next = new();

        public Trie()
        {
        }

        public Trie(Dictionary<string, TrieCandidate> candidates)
        {
            foreach (var (key, (tokenKind, value)) in candidates)
            {
                Add(key, tokenKind, value);
            }
        }

        public TrieCandidate? TerminalCandidate { get; private set; }

        public IReadOnlyDictionary<TokenKind, List<object>> Candidates => _candidateKeys;

        public bool TopCandidate(TokenKind tokenKind, out object? value)
        {
            value = _candidateKeys.TryGetValue(tokenKind, out var list) ? list.FirstOrDefault() : null;
            return value != null;
        }

        public void Add(string fullKey, TokenKind tokenKind, object value)
        {
            var currentTrie = this;
            currentTrie._candidateKeys.Add(tokenKind, value);

            foreach (var keyChar in fullKey)
            {
                if (!currentTrie._next.TryGetValue(keyChar, out var subTrie))
                {
                    subTrie = new Trie();
                    currentTrie._next[keyChar] = subTrie;
                }

                subTrie._candidateKeys.Add(tokenKind, value);
                currentTrie = subTrie;
            }

            currentTrie.TerminalCandidate = (tokenKind, value);
        }

        public bool TryNext(char keyChar, out Trie subTrie)
        {
            return _next.TryGetValue(keyChar, out subTrie);
        }
    }
}