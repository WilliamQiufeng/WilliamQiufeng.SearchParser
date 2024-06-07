using System;

namespace WilliamQiufeng.SearchParser.Parsing;

[Flags]
public enum SingletonEnumPolicy
{
    Disabled = 0,
    Enabled = 1 << 0,

    /// <summary>
    ///     For singleton enums, require complete names to be specified in order to recognize it as an enum
    /// </summary>
    RequireCompleteEnum = 1 << 1,

    /// <summary>
    ///     Include the exact content of the enum in the plain text search terms
    /// </summary>
    IncludeInPlainText = 1 << 2,

    /// <summary>
    ///     By default, singleton enums are allowed and would require complete names to identify themselves.
    ///     Once recognized, they are not included in plain text search terms.
    /// </summary>
    Default = Enabled | RequireCompleteEnum
}