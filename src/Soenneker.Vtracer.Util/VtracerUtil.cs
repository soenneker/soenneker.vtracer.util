using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Vtracer.Util.Abstract;
using Soenneker.Vtracer.Util.Commands;
using Soenneker.Vtracer.Util.Commands.Abstract;
using Soenneker.Vtracer.Util.Options;
using Soenneker.Utils.Directory.Abstract;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.Process.Abstract;
using Soenneker.Utils.Paths.Resources.Abstract;

namespace Soenneker.Vtracer.Util;

public sealed class VtracerUtil(IProcessUtil processUtil, IDirectoryUtil directoryUtil, IFileUtil fileUtil,
    IResourcesPathUtil resourcesPathUtil) : IVtracerUtil
{
    public async ValueTask<List<string>> Run(string arguments, string? workingDirectory = null, bool log = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(arguments);
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64 || (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux()))
            throw new PlatformNotSupportedException("Soenneker.Vtracer.Util supports Windows x64 and Linux x64.");

        string relativePath = OperatingSystem.IsWindows()
            ? Path.Combine("win-x64", "vtracer", "vtracer.exe")
            : Path.Combine("linux-x64", "vtracer", "vtracer");
        string executablePath = await resourcesPathUtil.GetResourceFilePath(relativePath, cancellationToken);
        if (!await fileUtil.Exists(executablePath, cancellationToken))
            throw new FileNotFoundException("The bundled VTracer executable was not found.", executablePath);

        if (OperatingSystem.IsLinux())
        {
            UnixFileMode mode = File.GetUnixFileMode(executablePath);
            UnixFileMode executableMode = mode | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
            if (mode != executableMode)
                File.SetUnixFileMode(executablePath, executableMode);
        }

        Dictionary<string, string>? environment = null;
        if (OperatingSystem.IsWindows())
        {
            string runtimePath = await resourcesPathUtil.GetResourceFilePath(
                Path.Combine("win-x64", "vcruntime", "vcruntime140.dll"), cancellationToken);
            if (!await fileUtil.Exists(runtimePath, cancellationToken))
                throw new FileNotFoundException("The bundled Visual C++ runtime was not found.", runtimePath);
            string runtimeDirectory = Path.GetDirectoryName(Path.GetFullPath(runtimePath))!;
            environment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["PATH"] = runtimeDirectory + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH")
            };
        }

        return await processUtil.Start(executablePath, workingDirectory, arguments, log: log,
            environmentalVars: environment, cancellationToken: cancellationToken);
    }

    public ValueTask<List<string>> Execute(IVtracerCommand command, string? workingDirectory = null, bool log = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        return Run(VtracerCommand.Build(command), workingDirectory, log, cancellationToken);
    }

    public async ValueTask<string> GetVersion(CancellationToken cancellationToken = default)
    {
        List<string> output = await Run("--version", log: false, cancellationToken: cancellationToken);
        return output.Count == 0 ? string.Empty : output[0];
    }

    public async ValueTask Convert(string inputPath, string outputPath, VtracerOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        string input = Path.GetFullPath(inputPath);
        string output = Path.GetFullPath(outputPath);
        if (!await fileUtil.Exists(input, cancellationToken))
            throw new FileNotFoundException("The input image was not found.", input);
        if (!Path.GetExtension(output).Equals(".svg", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The output path must have the .svg extension.", nameof(outputPath));
        if (string.Equals(input, output, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new ArgumentException("Input and output paths must differ.", nameof(outputPath));

        options?.Validate();
        string outputDirectory = Path.GetDirectoryName(output)!;
        await directoryUtil.Create(outputDirectory, cancellationToken: cancellationToken);
        string temporaryOutput = Path.Combine(outputDirectory, $".vtracer-{Guid.NewGuid():N}.svg");
        try
        {
            IVtracerCommand command = new VtracerCommand().AddOption("input", input).AddOption("output", temporaryOutput);
            options?.Apply(command);
            await Execute(command, outputDirectory, log: false, cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!File.Exists(temporaryOutput) || new FileInfo(temporaryOutput).Length == 0)
                throw new InvalidDataException("VTracer did not produce a nonempty SVG.");
            File.Move(temporaryOutput, output, overwrite: true);
        }
        finally
        {
            File.Delete(temporaryOutput);
        }
    }
}
