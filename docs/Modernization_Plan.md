# Rhino Mocks Modernization Plan

## Overview
This plan outlines key improvements to align Rhino Mocks with modern .NET practices and ensure robust support for .NET 8.0.

---

## 1. Dependency Upgrade
**Action**: Upgrade `Castle.Core` from 4.3.1 to latest stable (≥5.0)  
**Rationale**: 
- Castle.Core 4.3.1 (2019) lacks security patches and .NET 8.0 compatibility
- Modern DynamicProxy features improve proxy generation reliability

**Verification Steps**:
1. Confirm compatibility with `Rhino.Mocks`' proxy interception logic
2. Test all core scenarios with upgraded dependency

---

## 2. Nullability Enablement
**Action**: Enable `<Nullable>enable</Nullable>` centrally in `Directory.Build.props` rather than repeating it across project files; if needed, gate it with `Condition="'$(UsingMicrosoftNETSdk)' == 'true'"` for SDK-style projects only.  
**Rationale**:
- Helps reduce null reference exceptions in legacy codebases by surfacing null-safety issues earlier
- Modern .NET best practice since C# 8.0 (2019)
- Centralizing the setting in `Directory.Build.props` reduces drift and maintenance overhead across multiple projects

**Impact**:
- Public API will require nullability annotations
- Existing callers will need to update null-safe usage
- Project-specific exceptions, if any, should be handled as explicit overrides rather than duplicating the default everywhere

---

## 3. API Modernization
**Action**: Remove or relax `DOTNET35` conditional gating around the AAA API and clean up related `DefineConstants` usage  
**Rationale**:
- .NET 3.5 is obsolete (EOL 2017)
- AAA extension methods are currently gated behind the `DOTNET35` compilation symbol, even when targeting newer TFMs

**Implementation**:
```diff
- #if DOTNET35
  public static void Expect(...) { ... }
- #endif
```
Replace with:
```csharp
public static void Expect(...) { ... } // No conditional
```

---

## 4. Critical TODO Remediation
**Action**: Replace `Marshal.GetExceptionCode()` in `Rhino.Mocks/MockRepositoryRecordPlayback.cs`  
**Current Issue**:  
`DisposableActionsHelper.ExceptionWasThrownAndDisposableActionShouldNotBeCalled()` calls `Marshal.GetExceptionCode()` (obsolete, CS0618) to detect whether an exception is in-flight during `Dispose()` — not to read an error code or HRESULT. This call has no equivalent on Mono (already skipped) and no direct equivalent on .NET 8.0.  
**Note**: `Exception.HResult` is not applicable here because no exception instance is available in this context; the purpose is to determine whether *any* exception is currently unwinding the stack.  

**Proposed Approaches** (pick one):
1. **Refactor the API**: Introduce an explicit `Commit()` or `Complete()` method so callers signal success rather than relying on stack-unwind detection in `Dispose()`. This is the cleanest long-term solution.
2. **Accept a behavior change**: Remove the unwind detection entirely and always run `VerifyAll()` / replay in `Dispose()`. Document that callers must not rely on automatic suppression when an exception is in-flight.

**Call sites to update**:
- `Rhino.Mocks/MockRepositoryRecordPlayback.cs` — `DisposableActionsHelper.ExceptionWasThrownAndDisposableActionShouldNotBeCalled()`
- `Rhino.Mocks/Rhino.Mocks.csproj` — `GlobalSuppressions.cs` suppression and associated TODO comment

---

## 5. Documentation Modernization
**Required Updates**:
| Section | Before | After |
|---------|--------|-------|
| README | .NET 4.x-centric | .NET 8.0 setup guide |
| GettingStarted | Legacy sample | C# 10+ code samples |
| NuGet Docs | Basic description | Modern usage examples |

---

## 6. CI/CD Enhancement
**Additions**:
1. macOS/Linux test runners in `.github/workflows/main.yml`
2. Coverage reporting with `coverlet` for .NET 8.0
3. Automated dependency scanning (Snyk/Dependabot)

---

## Timeline
| Phase | Duration | Deliverable |
|-------|----------|-------------|
| Dependency Audit | 2 weeks | Upgrade path confirmation |
| Nullability Migration | 3 weeks | Nullable-enabled build |
| API Cleanup | 1 week | DOTNET35-free master branch |
| Final Validation | 1 week | Verified .NET 8.0 release |