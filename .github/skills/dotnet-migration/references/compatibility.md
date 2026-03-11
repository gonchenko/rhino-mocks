# .NET Migration Compatibility Reference

## TFM Preprocessor Symbols

| Target Framework | Defined symbols |
|-----------------|-----------------|
| net461 | `NETFRAMEWORK`, `NET461`, `NET461_OR_GREATER` |
| net48  | `NETFRAMEWORK`, `NET48`, `NET48_OR_GREATER` |
| netcoreapp3.1 | `NETCOREAPP`, `NETCOREAPP3_1`, `NETCOREAPP3_1_OR_GREATER` |
| net5.0 | `NET`, `NET5_0`, `NET5_0_OR_GREATER` |
| net6.0 | `NET`, `NET6_0`, `NET6_0_OR_GREATER` |
| net8.0 | `NET`, `NET8_0`, `NET8_0_OR_GREATER` |

Custom symbols can be added via `<DefineConstants>` in a TFM-conditioned `PropertyGroup`.

## Removed / Unavailable APIs

### .NET Remoting (not in .NET Core at all)

| Type / Namespace | Status | Alternative |
|------------------|--------|-------------|
| `System.Runtime.Remoting.*` | Removed | None in-box; use gRPC / named pipes |
| `MarshalByRefObject` transparent proxies | Removed | Castle DynamicProxy, DispatchProxy |
| `IChannel`, `IMessageSink` | Removed | — |
| `RemotingServices` | Removed | — |
| `ContextBoundObject` | Removed | — |

Pattern used in this repo:
```xml
<!-- Exclude remoting source files when building for net8.0 -->
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <Compile Remove="Impl\RemotingMock\*.cs" />
</ItemGroup>
```

### AppDomain

| API | Status | Alternative |
|-----|--------|-------------|
| `AppDomain.CreateDomain()` | Removed | `AssemblyLoadContext` |
| `AppDomain.GetAssemblies()` | Available | — |
| `AppDomain.BaseDirectory` | Available | — |
| `AppDomain.SetData` / `GetData` | Available | — |
| `Evidence`, `AppDomainSetup.PrivateBinPath` | Partial / removed | — |

### WCF (optional)

| Component | Status | Alternative |
|-----------|--------|-------------|
| `System.ServiceModel` (server) | Not in .NET Core inbox | [`CoreWCF`](https://github.com/CoreWCF/CoreWCF) — `CoreWCF.Http`, `CoreWCF.NetTcp` |
| `System.ServiceModel` (client) | Available as NuGet | `System.ServiceModel.Http`, `System.ServiceModel.NetTcp` |
| `WSHttpBinding`, `NetTcpBinding` | Client NuGet only | CoreWCF for server-side equivalents |
| `WebHttpBinding` (REST/JSON) | Not recommended | Replace with `HttpClient` + `System.Text.Json` |

### Serialization

| API | Status | Alternative |
|-----|--------|-------------|
| `BinaryFormatter` | Disabled (throws) in .NET 8 | `System.Text.Json`, `XmlSerializer`, `MessagePack` |
| `SoapFormatter` | Removed | `System.Text.Json` |
| `NetDataContractSerializer` | Removed | `DataContractSerializer` + known types |

### Security / Permissions

| API | Status | Alternative |
|-----|--------|-------------|
| `SecurityPermission`, `FileIOPermission`, etc. | No-ops or throw on .NET Core | Remove CAS calls; use OS-level security |
| `PermissionSet` | Available but no-op | — |
| `System.Security.Permissions` NuGet | Shim available | Add `<PackageReference Include="System.Security.Permissions">` |

### Reflection / Emit

| API | Status | Note |
|-----|--------|------|
| `AssemblyBuilder.DefineDynamicAssembly` with `RunAndSave` | `RunAndSave` removed | Use `PersistedAssemblyBuilder` (.NET 9+) or Mono.Cecil |
| `ReflectionOnlyLoad` | Removed | Use `MetadataLoadContext` NuGet |

### Threading / Synchronization

| API | Status | Alternative |
|-----|--------|-------------|
| `Thread.Abort()` | Throws `PlatformNotSupportedException` | Use `CancellationToken` |
| `Thread.Suspend` / `Resume` | Throws | Redesign with async/await |
| `AppDomain.UnhandledException` per-domain | Single domain model | Use global handler |

### Windows-only APIs

Guarding pattern:
```csharp
#if WINDOWS
    Registry.SetValue(...);
#endif
// or at runtime:
if (OperatingSystem.IsWindows()) { ... }
```

Add analyzer enforcement:
```xml
<EnableNETAnalyzers>true</EnableNETAnalyzers>
<AnalysisMode>Recommended</AnalysisMode>
```

## Common Build Errors and Fixes

| Error / Warning | Cause | Fix |
|-----------------|-------|-----|
| `CS0618` — `Marshal.GetExceptionCode()` obsolete | Deprecated API | Replace with `Marshal.GetHRForException` or suppress with `#pragma warning disable CS0618` |
| `SYSLIB0011` — `BinaryFormatter` is obsolete | BinaryFormatter disabled | Replace serialization strategy |
| `SYSLIB0003` — `CAS` is not supported | Code Access Security | Remove CAS attributes/calls |
| `SYSLIB0050` — `Formatter` obsolete | `IFormatter` | Migrate to modern serializer |
| `PlatformNotSupportedException` at runtime | Windows-only API on Linux/macOS | Guard with `OperatingSystem.IsWindows()` |
| `TypeLoadException` — type not found | Assembly not loaded | Check TFM-conditional references |

## SDK-Style .csproj Patterns

### Multi-targeting with conditional file exclusion
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net48;net8.0</TargetFrameworks>
  </PropertyGroup>

  <!-- Suppress a specific warning only on net8.0 -->
  <PropertyGroup Condition="'$(TargetFramework)' == 'net8.0'">
    <NoWarn>$(NoWarn);CS0618</NoWarn>
  </PropertyGroup>

  <!-- Exclude Framework-only source files from net8.0 build -->
  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
    <Compile Remove="Legacy\**\*.cs" />
  </ItemGroup>

  <!-- Reference a library only on net48 -->
  <ItemGroup Condition="'$(TargetFramework)' == 'net48'">
    <Reference Include="SomeFrameworkLib">
      <HintPath>..\SharedLibs\SomeFrameworkLib.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

### Per-TFM NuGet packages
```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <PackageReference Include="System.Security.Permissions" Version="8.0.0" />
</ItemGroup>
```

## Useful Tools

### Automated Refactoring

| Tool | Purpose |
|------|---------|
| Roslyn quick-fix (`Ctrl+.` / `dotnet format --diagnostics`) | Apply built-in fixers for SYSLIB / CS diagnostics across the project |
| `ast-grep` (`sg`) | Structural find-and-replace using AST patterns — no Roslyn setup required; great for call-site rewrites |
| Custom `ICodeFixProvider` (Roslyn SDK) | Write a reusable fixer when the same pattern recurs across many repos |
| `.NET Upgrade Assistant` (`dotnet-upgrade-assistant`) | First-pass automated project/package migration |

### Analysis & Runtime

| Tool | Purpose |
|------|---------|
| `dotnet-apicompat` | Detect binary compatibility breaks between TFMs |
| `Microsoft.DotNet.PlatformAbstractions` | Runtime OS detection helpers |
| `Polly` | Resilience patterns replacing Framework retry infra |
| `Castle.Core` / `DispatchProxy` | Proxy generation replacing Remoting proxies |
