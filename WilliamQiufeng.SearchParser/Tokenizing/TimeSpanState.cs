using System;
using System.Collections.Generic;
using System.Linq;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class TimeSpanState : ITokenizerState
    {
        public static readonly Trie<TimeSpanKind> Trie = new Trie<TimeSpanKind>(new Dictionary<string, TimeSpanKind>
        {
            ["hours"] = TimeSpanKind.Hour,
            ["hrs"] = TimeSpanKind.Hour,
            ["minutes"] = TimeSpanKind.Minute,
            ["seconds"] = TimeSpanKind.Second
        });

        private int _currentInteger;

        private Trie<TimeSpanKind>? _currentTrie;
        private int _hours = -1;
        private int _minutes = -1;
        private int _seconds = -1;

        public TimeSpanState(int currentInteger)
        {
            _currentInteger = currentInteger;
        }

        public ITokenizerState Process(Tokenizer tokenizer)
        {
            var lookahead = tokenizer.Lookahead();

            // We have reached the end: emit a token
            if (lookahead == '\0' || lookahead == ' ')
            {
                if (!SetField())
                    return PlainTextState.State;

                if (_hours == -1) _hours = 0;
                if (_minutes == -1) _minutes = 0;
                if (_seconds == -1) _seconds = 0;

                tokenizer.EmitToken(TokenKind.TimeSpan, new TimeSpan(_hours, _minutes, _seconds));
                return EmptyState.State;
            }

            if (_currentTrie != null)
            {
                if (_currentTrie.TryNext(lookahead, out var subTrie))
                {
                    tokenizer.Consume();
                    _currentTrie = subTrie;
                    return this;
                }

                SetField();
            }
            else if (Trie.TryNext(lookahead, out var subTrie))
            {
                tokenizer.Consume();
                _currentTrie = subTrie;
                return this;
            }

            if (lookahead < '0' || lookahead > '9')
                return PlainTextState.State;

            _currentInteger = _currentInteger * 10 + lookahead - '0';
            tokenizer.Consume();
            return this;
        }

        private bool SetField()
        {
            if (_currentTrie == null)
                return false;

            switch (_currentTrie.Candidates.First())
            {
                case TimeSpanKind.Hour when _hours == -1:
                    _hours = _currentInteger;
                    break;
                case TimeSpanKind.Minute when _minutes == -1:
                    _minutes = _currentInteger;
                    break;
                case TimeSpanKind.Second when _seconds == -1:
                    _seconds = _currentInteger;
                    break;
                default:
                    // Multiple indication of a time span component (e.g. 12s12s)
                    return false;
            }

            _currentInteger = 0;
            _currentTrie = null;
            return true;
        }
    }
}