using System;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    [Flags]
    public enum KeyEnumResolveMode
    {
        PlainTextOnly = 0,
        Key = 1 << 0,
        Enum = 1 << 1,
        Both = Key | Enum
    }
}