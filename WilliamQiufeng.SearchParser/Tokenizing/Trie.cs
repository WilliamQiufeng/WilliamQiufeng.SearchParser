using System.Collections.Generic;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class Trie
    {
        private readonly List<string> _candidateKeys = new List<string>();
        private readonly Dictionary<char, Trie> _next = new Dictionary<char, Trie>();

        public IReadOnlyCollection<string> Candidates => _candidateKeys;

        public void Add(string fullKey, int index = 0)
        {
            _candidateKeys.Add(fullKey);
            if (index == fullKey.Length)
                return;
            var keyChar = fullKey[index];
            if (!_next.TryGetValue(keyChar, out var subTrie))
            {
                subTrie = new Trie();
                _next[keyChar] = subTrie;
            }

            subTrie.Add(fullKey, index + 1);
        }

        public bool TryNext(char keyChar, out Trie subTrie)
        {
            return _next.TryGetValue(keyChar, out subTrie);
        }
    }
}