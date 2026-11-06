using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Vtracer.Util.Commands.Abstract;
using Soenneker.Vtracer.Util.Options;

namespace Soenneker.Vtracer.Util.Abstract;

/// <summary>Runs the bundled VTracer CLI on Windows x64 or Linux x64.</summary>
public interface IVtracerUtil
{
    /// <summary>Runs raw CLI arguments and returns output lines. A nonzero exit code throws.</summary>
    /// <param name="arguments">Arguments passed directly to VTracer, without a shell.</param>
    /// <param name="workingDirectory">Optional process working directory.</param>
    /// <param name="log">Whether to log process output.</param>
    /// <param name="cancellationToken">Cancels the running process.</param>
    ValueTask<List<string>> Run(string arguments, string? workingDirectory = null, bool log = true, CancellationToken cancellationToken = default);

    /// <summary>Runs a structured command with escaped arguments and invariant option values.</summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="workingDirectory">Optional process working directory.</param>
    /// <param name="log">Whether to log process output.</param>
    /// <param name="cancellationToken">Cancels the running process.</param>
    ValueTask<List<string>> Execute(IVtracerCommand command, string? workingDirectory = null, bool log = true, CancellationToken cancellationToken = default);

    /// <summary>Gets the bundled executable's version.</summary>
    ValueTask<string> GetVersion(CancellationToken cancellationToken = default);

    /// <summary>Converts an image to SVG. Replaces the destination only after a successful conversion.</summary>
    /// <param name="inputPath">A raster image supported by the bundled VTracer release.</param>
    /// <param name="outputPath">Destination ending in .svg. Parent directories are created if necessary.</param>
    /// <param name="options">Optional tracing settings; null preserves upstream defaults.</param>
    /// <param name="cancellationToken">Cancels conversion and leaves the existing destination intact.</param>
    ValueTask Convert(string inputPath, string outputPath, VtracerOptions? options = null, CancellationToken cancellationToken = default);
}
