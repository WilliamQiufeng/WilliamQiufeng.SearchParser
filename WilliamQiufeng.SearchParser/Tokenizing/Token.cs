using System;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class Token
    {
        public readonly int Offset;
        public readonly ReadOnlyMemory<char> Segment;

        /// <summary>
        ///     Whether the token is a part of a search criterion
        /// </summary>
        public bool IncludedInCriterion;

        /// <summary>
        ///     If <see cref="TokenKind"/> is <see cref="TokenKind.Enum"/>, whether the enum is specified fully.
        ///     For example, this will be marked true if there is an enum "tag" and the content of this token is "tag".
        ///     If the content is "t" or "ta" instead, it will be false.
        /// </summary>
        public bool IsCompleteKeyword;

        public TokenKind Kind;
        public object? Value;

        public Token()
        {
        }

        public Token(TokenKind kind, ReadOnlyMemory<char> segment = default, int offset = 0, object? content = default)
        {
            Kind = kind;
            Segment = segment;
            Offset = offset;
            Value = content;
            IncludedInCriterion = false;
        }

        public override string ToString()
        {
            return $"[{Kind} {Segment.ToString()} ({Value}) #{Offset}]";
        }

        /// <summary>
        ///     Tries to determine the exact token kind of this keyword.
        ///     If there are any candidates that match the expected <see cref="tokenKind"/>,
        ///     <see cref="Kind"/> and <see cref="Value"/> will be set to the corresponding values set in <see cref="Tokenizer.KeywordTrie"/>.
        /// </summary>
        /// <param name="tokenKind">The kind of keyword to try to collapse</param>
        /// <param name="requireCompleteKeyword">Whether a complete name is needed to identify itself</param>
        /// <returns>Whether the conversion is successful</returns>
        public bool TryCollapseKeyword(TokenKind tokenKind, bool requireCompleteKeyword)
        {
            if (Kind == tokenKind) return true;
            if (Kind != TokenKind.Keyword) return false;

            if (tokenKind == TokenKind.PlainText)
            {
                CollapseToPlainText();
                return true;
            }

            if (requireCompleteKeyword && !IsCompleteKeyword) return false;

            if (Value is not Trie trie) return false;

            // Prioritize terminal candidates (full keyword) over top candidate matches
            if (trie.TerminalCandidate is var (foundTokenKind, value) && foundTokenKind == tokenKind
                || trie.TopCandidate(tokenKind, out value))
            {
                Kind = tokenKind;
                Value = value;
                return true;
            }

            return false;
        }

        public void CollapseToPlainText()
        {
            Kind = TokenKind.PlainText;
            Value = Segment.ToString();
        }
    }
}