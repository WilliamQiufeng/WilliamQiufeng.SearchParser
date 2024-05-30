using System;

namespace WilliamQiufeng.SearchParser.Parsing;

[Flags]
public enum StrandedEnumPolicy
{
    Disabled = 0,
    Enabled = 1 << 0,

    /// <summary>
    ///     For stranded enums, require complete names to be specified in order to recognize it as an enum
    /// </summary>
    RequireCompleteEnum = 1 << 1,

    /// <summary>
    ///     Include the exact content of the enum in the plain text search terms
    /// </summary>
    IncludeInPlainText = 1 << 2,
    Default = Enabled | RequireCompleteEnum | IncludeInPlainText
}