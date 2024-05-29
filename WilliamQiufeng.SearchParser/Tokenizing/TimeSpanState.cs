using System;
using System.Collections.Generic;
using System.Linq;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class TimeSpanState : ITokenizerState
    {
        public static readonly Trie<TimeSpanKind> Trie = new(new Dictionary<string, TimeSpanKind>
        {
            ["hours"] = TimeSpanKind.Hour,
            ["hrs"] = TimeSpanKind.Hour,
            ["minutes"] = TimeSpanKind.Minute,
            ["seconds"] = TimeSpanKind.Second
        });

        private int _currentInteger;

        private Trie<TimeSpanKind>? _currentTrie;
        private FormatKind _formatKind = FormatKind.Unknown;
        private bool _hasAnyDigitRead = true;
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
            var isDigit = lookahead is >= '0' and <= '9';

            // We have reached the end: emit a token
            if (lookahead is '\0' or ' ')
            {
                // Finish up: record the last component
                // '5:' (no digits for last component) and '1m25' (no unit) fails here
                if (!SetField())
                    return PlainTextState.State;

                if (_hours == -1) _hours = 0;
                if (_minutes == -1) _minutes = 0;
                if (_seconds == -1) _seconds = 0;

                tokenizer.EmitToken(TokenKind.TimeSpan, new TimeSpan(_hours, _minutes, _seconds));
                return EmptyState.State;
            }

            // Next character is a unit
            var trieNextMatch = (_currentTrie ?? Trie).TryNext(lookahead, out var subTrie);
            if (_formatKind is FormatKind.Unknown or FormatKind.Unit && trieNextMatch)
            {
                tokenizer.Advance();
                _currentTrie = subTrie;
                _formatKind = FormatKind.Unit;
                return this;
            }

            // We are just done reading a unit (next character isn't in trie's key)
            if (_formatKind == FormatKind.Unit && _currentTrie != null)
                SetFieldUnit();

            // Next character is a colon ':'
            if (_formatKind is FormatKind.Unknown or FormatKind.Colon && lookahead == ':')
            {
                tokenizer.Advance();
                _formatKind = FormatKind.Colon;
                // We got a duplicate colon, or SetFieldColon fails
                // '1::1' and '1:1:1:1' fails here
                if (!_hasAnyDigitRead || !SetFieldColon())
                    return PlainTextState.State;
                return this;
            }

            if (isDigit)
            {
                tokenizer.Advance();
                _currentInteger = _currentInteger * 10 + lookahead - '0';
                _hasAnyDigitRead = true;
                return this;
            }

            // '1ms' fails here
            return PlainTextState.State;
        }

        private bool SetField()
        {
            if (!_hasAnyDigitRead)
                return false;

            switch (_formatKind)
            {
                case FormatKind.Colon:
                    return SetFieldColon();
                case FormatKind.Unit:
                    return SetFieldUnit();
                case FormatKind.Unknown:
                default:
                    return false;
            }
        }

        private bool SetFieldUnit()
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
            _hasAnyDigitRead = false;
            return true;
        }

        private bool SetFieldColon()
        {
            if (_hours != -1)
                return false;
            _hours = _minutes;
            _minutes = _seconds;
            _seconds = _currentInteger;
            _currentInteger = 0;
            _hasAnyDigitRead = false;
            return true;
        }

        private enum FormatKind
        {
            Unknown,
            Colon,
            Unit
        }
    }
}