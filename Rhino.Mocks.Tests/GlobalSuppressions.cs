// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.

using System.Diagnostics.CodeAnalysis;

// Test code calls string.StartsWith/EndsWith against single-char strings that are tested
// as invariant predicates; culture-aware comparison is not relevant.
[assembly: SuppressMessage("Globalization", "CA1310:Specify StringComparison for correctness",
    Justification = "Test predicates compare against fixed single-char strings; culture-aware comparison is not needed.")]

// Prefer char overload: test lambdas use the string overload for readability; the
// performance difference is negligible in a test context.
[assembly: SuppressMessage("Performance", "CA1866:Use 'string.Method(char)' instead of 'string.Method(string)'",
    Justification = "Test lambdas; the string overload is acceptable for readability.")]

// BinaryFormatter deserialization used only in the legacy Coverage.SerializeAndDeserialize
// helper, guarded by #if !NET8_0.  Trusted in-process data only.
[assembly: SuppressMessage("Security", "CA2300:Do not use insecure deserializer BinaryFormatter",
    Justification = "Legacy test helper; data is a trusted in-process serialization roundtrip, not user input.",
    Scope = "member",
    Target = "~M:Rhino.Mocks.Tests.Coverage.SerializeAndDeserialize(System.Object)~System.Object")]

[assembly: SuppressMessage("Security", "CA2301:Do not call BinaryFormatter.Deserialize without first setting BinaryFormatter.Binder",
    Justification = "Legacy test helper; data is a trusted in-process serialization roundtrip, not user input.",
    Scope = "member",
    Target = "~M:Rhino.Mocks.Tests.Coverage.SerializeAndDeserialize(System.Object)~System.Object")]
