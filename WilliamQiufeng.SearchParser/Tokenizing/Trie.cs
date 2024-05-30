using System;
using System.Collections.Generic;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class Trie<TCandidate>
    {
        private readonly List<TCandidate> _candidateKeys = [];
        private readonly Dictionary<char, Trie<TCandidate>> _next = new();

        public Trie()
        {
        }

        public Trie(IDictionary<string, TCandidate> candidates)
        {
            foreach (var (key, value) in candidates)
            {
                Add(key, value);
            }
        }

        public TCandidate? TerminalCandidate { get; private set; }

        public IReadOnlyCollection<TCandidate> Candidates => _candidateKeys;

        public void Add(string fullKey, TCandidate value)
        {
            var currentTrie = this;
            currentTrie._candidateKeys.Add(value);

            foreach (var keyChar in fullKey)
            {
                if (!currentTrie._next.TryGetValue(keyChar, out var subTrie))
                {
                    subTrie = new Trie<TCandidate>();
                    currentTrie._next[keyChar] = subTrie;
                }

                subTrie._candidateKeys.Add(value);
                currentTrie = subTrie;
            }

            currentTrie.TerminalCandidate = value;
        }

        public bool TryNext(char keyChar, out Trie<TCandidate> subTrie)
        {
            return _next.TryGetValue(keyChar, out subTrie);
        }

        public bool TryNext(ReadOnlySpan<char> segment, out Trie<TCandidate> subTrie)
        {
            subTrie = this;
            foreach (var c in segment)
                if (!subTrie.TryNext(c, out subTrie))
                    return false;

            return true;
        }
    }
}