using System;
using System.Collections.Generic;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class Trie<TCandidate>
    {
        private readonly List<TCandidate> _candidateKeys = new();
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

        public IReadOnlyCollection<TCandidate> Candidates => _candidateKeys;

        public void Add(string fullKey, TCandidate value, int index = 0)
        {
            _candidateKeys.Add(value);
            if (index == fullKey.Length)
                return;
            var keyChar = fullKey[index];
            if (!_next.TryGetValue(keyChar, out var subTrie))
            {
                subTrie = new Trie<TCandidate>();
                _next[keyChar] = subTrie;
            }

            subTrie.Add(fullKey, value, index + 1);
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