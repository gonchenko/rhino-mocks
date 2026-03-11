---
name: dotnet-migration
description: 'Migrate C# code from .NET Framework to .NET Core / .NET 5+. Use when: porting libraries, multi-targeting net48 and net8.0+, removing Framework-only APIs (Remoting, AppDomain, BinaryFormatter, WCF), converting old .csproj format, fixing conditional compilation directives (#if NETFRAMEWORK, #if NET48), replacing removed BCL types, updating NuGet packages, handling test framework differences across TFMs.'
argument-hint: 'Describe the class, project, or API surface to migrate'
---

# .NET Framework → .NET Core Migration

## When to Use
- Converting an old-style (non-SDK) `.csproj` to SDK-style with multi-targeting
- Adding `net8.0` (or newer) alongside an existing `net48` / `net461` target
- Identifying and replacing APIs removed or broken in .NET Core / .NET 5+
- Wrapping Framework-only code in `#if NETFRAMEWORK` or `#if NET48` guards
- Updating NuGet packages to cross-platform versions
- Fixing test projects to build and run under multiple TFMs

## Principles

**Prefer automated refactoring over manual edits.** When a replacement is mechanical and repetitive (e.g., swapping every `BinaryFormatter` call, adding `#if NETFRAMEWORK` guards around a class pattern, renaming namespaces), reach for an automated tool first:

| Scale | Tool | When to use |
|-------|------|-------------|
| Single file / IDE | Roslyn quick-fix (`Ctrl+.`) or `dotnet-codefix` | SYSLIB diagnostic has a built-in fixer; IDE surfaces it inline |
| Project-wide | `dotnet format` + Roslyn analyzers | Bulk-apply all pending fixers: `dotnet format --diagnostics SYSLIB0011` |
| Structural / cross-file patterns | `ast-grep` (`sg`) | Pattern-based rewrite without writing a full Roslyn analyzer; ideal for repeating call-site shapes |
| Large codebase | Custom `ICodeFixProvider` (Roslyn SDK) | When the pattern recurs across many repos or needs to ship as a team tool |
| Whole-project migration | `.NET Upgrade Assistant` | First-pass automation before manual cleanup |

Only fall back to manual edits when the pattern is irregular, context-sensitive, or occurs fewer than ~5 times.

## Procedure

### 1. Audit the Target

1. Check the current `TargetFramework(s)` in the `.csproj`.
2. Run a build against the new TFM and collect all errors and warnings:
   ```
   dotnet build -f net8.0
   ```
3. Note every broken symbol — group by category (Remoting, BinaryFormatter, WCF, AppDomain APIs, COM Interop, Windows-only, etc.).

### 2. Convert the Project File

If still using the old non-SDK format, convert first:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net48;net8.0</TargetFrameworks>
  </PropertyGroup>
</Project>
```

Key rules:
- Use `<TargetFrameworks>` (plural) for multi-targeting.
- Replace `<packages.config>` references with `<PackageReference>`.
- Remove `AssemblyInfo.cs` auto-generated properties (`Version`, `Copyright`, etc.) — the SDK generates them.
- Move strong-naming to `<SignAssembly>` + `<AssemblyOriginatorKeyFile>` in a `<PropertyGroup>`.
- Use per-TFM `<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">` to exclude Framework-only files.

### 3. Apply Conditional Compilation

| Scenario | Directive |
|----------|-----------|
| Code only valid on any .NET Framework | `#if NETFRAMEWORK` |
| Code only valid on net48 specifically | `#if NET48` |
| Code broken only on NET8+ | `#if !NET8_0_OR_GREATER` |
| Code valid on all modern TFMs | `#if NET5_0_OR_GREATER` |

Predefined TFM symbols follow the pattern `NETX_Y` and `NETX_Y_OR_GREATER`.  
See the [Compatibility Reference](./references/compatibility.md) for common symbol mappings.

### 4. Handle Removed APIs

Consult [Compatibility Reference](./references/compatibility.md) for the full list.  
Priority categories in order of frequency:

1. **Remoting** — `MarshalByRefObject`-based proxies, `IChannel`, `RemotingServices`:  
   Wrap entire files in `#if NETFRAMEWORK` or exclude via `<Compile Remove="..." />` in the `net8.0` item group.

2. **AppDomain** — `AppDomain.CreateDomain()`, `Evidence`, `AppDomainSetup`:  
   Replace with direct execution; use `AssemblyLoadContext` for isolation.

3. **BinaryFormatter** — disabled by default in .NET 8, throws `NotSupportedException`:  
   Replace with `System.Text.Json`, `MessagePack`, or `XmlSerializer`.

4. **WCF** — not in .NET Core inbox:  
   Use `CoreWCF` (server) or `System.ServiceModel.Http` NuGet (client).

5. **WCF** *(optional — skip if not used)* — not in .NET Core inbox:  
   - Server side: replace with [`CoreWCF`](https://github.com/CoreWCF/CoreWCF) NuGet packages.  
   - Client side: add `<PackageReference Include="System.ServiceModel.Http" />` (or `NetTcp`, `Duplex`).  
   - Wrap the service host / client proxy code in `#if NETFRAMEWORK` and provide a parallel implementation for .NET Core if CoreWCF migration is deferred.

6. **Windows-only APIs** — GDI+, WinForms, `Registry`, `EventLog`:  
   Guard with `[SupportedOSPlatform("windows")]` or `#if WINDOWS` / `OperatingSystem.IsWindows()`.

7. **Deprecated marshal helpers** — `Marshal.GetExceptionCode()` (CS0618):  
   Suppress with `<NoWarn>$(NoWarn);CS0618</NoWarn>` only when there is no supported alternative.

### 5. Update NuGet Packages

- Replace packages that only shipped for .NET Framework with their modern equivalents or inbox BCL types.
- Common replacements:

  | Old Package | Replacement |
  |-------------|-------------|
  | `System.Net.Http` (old) | Inbox in .NET Core |
  | `Newtonsoft.Json` (if tolerated) | `System.Text.Json` |
  | `log4net` / `NLog` | `Microsoft.Extensions.Logging` abstraction |
  | `System.Configuration.ConfigurationManager` | Keep as NuGet — it's cross-platform now |
  | `System.Security.Permissions` | Available as NuGet shim for compatibility |
  | `System.Diagnostics.EventLog` | Available as NuGet shim |

- Add per-TFM `<PackageReference>` when a reference is needed only on one TFM:
  ```xml
  <ItemGroup Condition="'$(TargetFramework)' == 'net48'">
    <Reference Include="YourLegacyLib" />
  </ItemGroup>
  ```

### 6. Fix Tests

1. Ensure the test project also multi-targets: `<TargetFrameworks>net48;net8.0</TargetFrameworks>`.
2. Wrap Framework-only tests in `#if NETFRAMEWORK` (class level is fine when the whole fixture is Framework-only, as in `RemotingMockTests.cs`).
3. Use `[Fact(Skip = "...")]` for individual tests that cannot run on .NET Core.
4. Run tests per TFM to verify both pass:
   ```
   dotnet test -f net48
   dotnet test -f net8.0
   ```

### 7. Verify the Build

```
dotnet build --configuration Release
dotnet test  --configuration Release --no-build
```

Check for:
- Zero `CS0618` / `SYSLIB` warnings that indicate unreviewed obsoletion.
- No `PlatformNotSupportedException` at runtime (shows as a test failure, not a build error).
- Assembly strong-naming consistent across both TFMs.

## Quick Checklist

- [ ] `.csproj` is SDK-style with `<TargetFrameworks>` (plural)
- [ ] Remoting / Reflection-only-load / AppDomain.CreateDomain code excluded from non-Framework TFMs
- [ ] `BinaryFormatter` usages replaced or removed
- [ ] All `PackageReference` versions resolve for every TFM
- [ ] Windows-only APIs guarded with `OperatingSystem.IsWindows()` or `[SupportedOSPlatform]`
- [ ] Tests build and pass under every TFM
- [ ] No unintentional `<NoWarn>` suppressing real issues
- [ ] Mechanical replacements done via Roslyn fix / ast-grep before any manual edits

## References

- [Compatibility Reference](./references/compatibility.md) — removed APIs, TFM symbols, common error codes
