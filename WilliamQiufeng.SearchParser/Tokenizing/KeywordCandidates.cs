using System.Collections.Generic;

namespace WilliamQiufeng.SearchParser.Tokenizing;

public class KeywordCandidates : Dictionary<TokenKind, List<object>>
{
    public void Add(TokenKind tokenKind, object candidate)
    {
        TryAdd(tokenKind, []);
        this[tokenKind].Add(candidate);
    }
}