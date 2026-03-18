// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// CS0618 (Marshal.GetExceptionCode obsolete) is suppressed globally for net8.0
// via <NoWarn> in the project file, as SuppressMessage does not apply to compiler warnings.

using System.Diagnostics.CodeAnalysis;

// The codebase calls string.StartsWith/EndsWith/IndexOf against fixed .NET naming-convention
// prefixes ("get_", "set_", "add_", "remove_") and user-supplied pattern strings that are
// intentionally treated with ordinal/culture-invariant semantics.  Migrating all call sites
// to pass StringComparison is tracked as a separate improvement; for now suppress the warning
// assembly-wide within this project.
[assembly: SuppressMessage("Globalization", "CA1310:Specify StringComparison for correctness",
    Justification = "Legacy code; string comparisons target .NET naming-convention prefixes and are culture-invariant by intent.")]
