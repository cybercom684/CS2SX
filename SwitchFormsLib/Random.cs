// CS2SX Stub für System.Random
// Ermöglicht dem Roslyn-SemanticModel Random-Aufrufe aufzulösen
// ohne echte BCL-Referenzen.

/// <summary>
/// CS2SX-Stub für System.Random.
/// Wird transpiliert zu CS2SX_Rand_Next / CS2SX_Rand_Float.
/// </summary>
public sealed class Random
{
    public static readonly Random Shared = new Random();

    public Random()
    {
    }
    public Random(int seed)
    {
    }

    /// <summary>Returns a non-negative random integer.</summary>
    public int Next() => 0;

    /// <summary>Returns a non-negative random integer less than maxValue.</summary>
    public int Next(int maxValue) => 0;

    /// <summary>Returns a random integer between minValue and maxValue.</summary>
    public int Next(int minValue, int maxValue) => 0;

    /// <summary>Returns a random float between 0.0 and 1.0.</summary>
    public float NextSingle() => 0f;

    /// <summary>Returns a random double between 0.0 and 1.0.</summary>
    public double NextDouble() => 0.0;
}