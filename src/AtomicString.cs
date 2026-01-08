using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Soenneker.Atomics.Strings;

/// <summary>
/// A lock-free, thread-safe wrapper for atomically publishing and reading a <see cref="string"/> reference.
/// Useful for shared string state and one-time (or racing) initialization without locks.
/// </summary>
public sealed class AtomicString
{
    private string? _value;

    /// <summary>
    /// Returns the current value (may be null).
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? Get() => Volatile.Read(ref _value);

    /// <summary>
    /// Returns the current value (may be null).
    /// </summary>
    [Pure]
    public string? Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Volatile.Read(ref _value);
    }

    /// <summary>
    /// Returns true when a non-null value has been published.
    /// </summary>
    [Pure]
    public bool HasValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Volatile.Read(ref _value) is not null;
    }

    /// <summary>
    /// Atomically sets the value and returns the previous value (may be null).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? Exchange(string? value) => Interlocked.Exchange(ref _value, value);

    /// <summary>
    /// Clears the value (sets to null) and returns the previous value (may be null).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? Clear() => Interlocked.Exchange(ref _value, null);

    /// <summary>
    /// Attempts to set the value only if the current value is null.
    /// Returns true if the caller won the race and published <paramref name="value"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySet(string value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        return Interlocked.CompareExchange(ref _value, value, null) is null;
    }

    /// <summary>
    /// Attempts to set the value only if the current value is null or empty.
    /// Returns true if the value was set by this call.
    /// </summary>
    public bool TrySetIfNullOrEmpty(string value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        while (true)
        {
            string? existing = Volatile.Read(ref _value);

            // If already a non-empty value, don't overwrite.
            if (!string.IsNullOrEmpty(existing))
                return false;

            // If it's null, try publish
            if (existing is null)
                return Interlocked.CompareExchange(ref _value, value, null) is null;

            // existing == "" (empty string): try swap "" -> value
            if (ReferenceEquals(Interlocked.CompareExchange(ref _value, value, existing), existing))
                return true;

            // else: raced, loop
        }
    }

    /// <summary>
    /// Atomically replaces the value if the current value reference equals <paramref name="comparand"/>.
    /// Returns the original value (may be null).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? CompareExchange(string? value, string? comparand) =>
        Interlocked.CompareExchange(ref _value, value, comparand);

    /// <summary>
    /// Atomically replaces the value if the current value reference equals <paramref name="comparand"/>.
    /// Returns true if the exchange occurred.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryCompareExchange(string? value, string? comparand) =>
        ReferenceEquals(Interlocked.CompareExchange(ref _value, value, comparand), comparand);

    /// <summary>
    /// Returns the current value if present; otherwise computes a value, publishes it (best-effort),
    /// and returns the published value.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetOrAdd(Func<string> factory)
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        string? existing = Volatile.Read(ref _value);
        if (existing is not null)
            return existing;

        return GetOrAddSlow(factory);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private string GetOrAddSlow(Func<string> factory)
    {
        // Re-check under contention
        string? existing = Volatile.Read(ref _value);
        if (existing is not null)
            return existing;

        string created = factory() ?? throw new InvalidOperationException("AtomicString factory returned null.");

        // Publish (races fine); return the winner
        Interlocked.CompareExchange(ref _value, created, null);
        return Volatile.Read(ref _value)!;
    }
}