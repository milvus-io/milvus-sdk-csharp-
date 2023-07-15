﻿using System.Globalization;
using IO.Milvus;

namespace IO.MilvusTests.Client;

/// <summary>
/// Milvus version
/// </summary>
public sealed record MilvusVersion
{
    private MilvusVersion(int major, int minor, int patch, string? suffix)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Suffix = suffix;
    }

    /// <summary>
    /// Major version
    /// </summary>
    public int Major { get; }

    /// <summary>
    /// Minor version
    /// </summary>
    public int Minor { get; }

    /// <summary>
    /// Patch version
    /// </summary>
    public int Patch { get; }

    /// <summary>
    /// Suffix version
    /// </summary>
    public string? Suffix { get; }

    /// <summary>
    /// Parse version string
    /// </summary>
    /// <param name="version">Version string</param>
    /// <returns>Milvus version</returns>
    public static MilvusVersion Parse(string version)
    {
        Verify.NotNull(version);

        string[] versions = version.Substring(1, version.Length - 1).Split('.', '-');
        return new MilvusVersion(
            int.Parse(versions[0], CultureInfo.InvariantCulture),
            int.Parse(versions[1], CultureInfo.InvariantCulture),
            int.Parse(versions[2], CultureInfo.InvariantCulture),
            version.Length > 3 ? null : versions[3]);
    }

    /// <summary>
    /// Compare version
    /// </summary>
    public bool GreaterThan(int major, int minor, int patch)
        => Major > major || Minor > minor || Patch > patch;

    /// <inheritdoc />
    public override string ToString()
        => string.IsNullOrEmpty(Suffix) ?
            $"{Major}.{Minor}.{Patch}" :
            $"{Major}.{Minor}.{Patch}-{Suffix}";
}
