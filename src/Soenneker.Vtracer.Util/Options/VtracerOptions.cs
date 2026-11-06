using System;
using Soenneker.Vtracer.Util.Commands.Abstract;

namespace Soenneker.Vtracer.Util.Options;

/// <summary>Optional VTracer CLI settings. Unspecified values preserve upstream defaults.</summary>
public sealed class VtracerOptions
{
    /// <summary>Starting preset: bw, poster, or photo.</summary>
    public string? Preset { get; set; }

    /// <summary>Region formation: color-cluster, bw, or watershed.</summary>
    public string? Clustering { get; set; }

    /// <summary>Layering: stacked or cutout.</summary>
    public string? Hierarchical { get; set; }

    /// <summary>Curve fitting: pixel, polygon, or spline.</summary>
    public string? Mode { get; set; }

    /// <summary>Discard patches smaller than this size, from 0 to 128.</summary>
    public int? FilterSpeckle { get; set; }

    /// <summary>Significant bits per color channel, from 1 to 8.</summary>
    public int? ColorPrecision { get; set; }

    /// <summary>Color difference between gradient layers, from 0 to 255.</summary>
    public int? GradientStep { get; set; }

    /// <summary>Nonnegative curve simplification tolerance in pixels.</summary>
    public double? Simplify { get; set; }

    /// <summary>Nonnegative number of decimal places in SVG path coordinates.</summary>
    public int? PathPrecision { get; set; }

    /// <summary>Output optimization level: 0, 1, or 2.</summary>
    public int? Optimize { get; set; }

    internal void Validate()
    {
        Choice(Preset, nameof(Preset), "bw", "poster", "photo");
        Choice(Clustering, nameof(Clustering), "color-cluster", "bw", "watershed");
        Choice(Hierarchical, nameof(Hierarchical), "stacked", "cutout");
        Choice(Mode, nameof(Mode), "pixel", "polygon", "spline");
        Range(FilterSpeckle, 0, 128, nameof(FilterSpeckle));
        Range(ColorPrecision, 1, 8, nameof(ColorPrecision));
        Range(GradientStep, 0, 255, nameof(GradientStep));
        Range(PathPrecision, 0, int.MaxValue, nameof(PathPrecision));
        Range(Optimize, 0, 2, nameof(Optimize));
        if (Simplify is { } tolerance && (!double.IsFinite(tolerance) || tolerance < 0))
            throw new ArgumentOutOfRangeException(nameof(Simplify));
    }

    internal void Apply(IVtracerCommand command)
    {
        Add(command, "preset", Preset);
        Add(command, "clustering", Clustering);
        Add(command, "hierarchical", Hierarchical);
        Add(command, "mode", Mode);
        Add(command, "filter-speckle", FilterSpeckle);
        Add(command, "color-precision", ColorPrecision);
        Add(command, "gradient-step", GradientStep);
        Add(command, "simplify", Simplify);
        Add(command, "path-precision", PathPrecision);
        Add(command, "optimize", Optimize);
    }

    private static void Add(IVtracerCommand command, string name, object? value)
    {
        if (value is not null)
            command.AddOption(name, value);
    }

    private static void Range(int? value, int minimum, int maximum, string name)
    {
        if (value is { } number && (number < minimum || number > maximum))
            throw new ArgumentOutOfRangeException(name, value, $"Must be between {minimum} and {maximum}.");
    }

    private static void Choice(string? value, string name, params string[] allowed)
    {
        if (value is not null && Array.IndexOf(allowed, value) < 0)
            throw new ArgumentException($"Must be one of: {string.Join(", ", allowed)}.", name);
    }
}
