[![](https://img.shields.io/nuget/v/soenneker.atomics.strings.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.atomics.strings/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.atomics.strings/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.atomics.strings/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.atomics.strings.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.atomics.strings/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.atomics.strings/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.atomics.strings/actions/workflows/codeql.yml)

# Soenneker.Atomics.Strings

A lock-free, thread-safe wrapper for atomically publishing and reading a `string` reference. Useful for shared string state and one-time (or racing) initialization without locks.

## Install

```bash
dotnet add package Soenneker.Atomics.Strings
```

## What you get

- `AtomicString` — A lock-free, thread-safe wrapper for atomically publishing and reading a `string` reference. Useful for shared string state and one-time (or racing) initialization without locks.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `AtomicString.Get()` | Returns the current value (may be null). | The current value. |
| `AtomicString.Value` | Returns the current value (may be null). | Returns the current value (may be null). |
| `AtomicString.HasValue` | Returns true when a non-null value has been published. | Returns true when a non-null value has been published. |
| `AtomicString.Exchange(value)` | Atomically sets the value and returns the previous value (may be null). | The value that was stored before the atomic update. |
| `AtomicString.Clear()` | Clears the value (sets to null) and returns the previous value (may be null). | The value that was stored before the atomic update. |
| `AtomicString.TrySet(value)` | Attempts to set the value only if the current value is null. Returns true if the caller won the race and published `value`. | true if the requested update was applied; otherwise, false. |
| `AtomicString.TrySetIfNullOrEmpty(value)` | Attempts to set the value only if the current value is null or empty. Returns true if the value was set by this call. | true if the requested update was applied; otherwise, false. |
| `AtomicString.CompareExchange(value, comparand)` | Atomically replaces the value if the current value reference equals `comparand`. Returns the original value (may be null). | The value observed before the compare-and-exchange attempt. |
| `AtomicString.TryCompareExchange(value, comparand)` | Atomically replaces the value if the current value reference equals `comparand`. Returns true if the exchange occurred. | true if atomically replaces the value if the current value reference equals . Returns true if the exchange occurred; otherwise, false. |
| `AtomicString.GetOrAdd(factory)` | Returns the current value if present; otherwise computes a value, publishes it (best-effort), and returns the published value. | The current value. |
