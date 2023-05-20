using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.Mahjong;

#nullable enable
public class LocalAkochanReviewer : IMajsoulAnalyzer, IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly string _workingDirectory;
    private readonly string _executablePath;

    public LocalAkochanReviewer(string workingDirectory, string executablePath)
    {
        _workingDirectory = workingDirectory;
        _executablePath = executablePath;
    }

    public bool IsIdle => _semaphore.CurrentCount == 1;

    public Task<byte[]> AnalyzeAsync(ReadOnlyMemory<byte> logJsonBytes, int targetActor, int[] ptList, double deviationThreshold, string id)
    {
        if (!_semaphore.Wait(0))
            throw new InvalidOperationException("Already analyzing.");

        return Task.Run(async () =>
        {
            try
            {
                var processStart = new ProcessStartInfo(_executablePath)
                {
                    FileName = _executablePath,
                    WorkingDirectory = _workingDirectory,
                    ArgumentList = { "-a", targetActor.ToString(), "--pt", string.Join(',', ptList), "-n", deviationThreshold.ToString(), "-o", "-" },
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Environment = { { "LD_LIBRARY_PATH", Path.Combine(_workingDirectory, "akochan") } },
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardInputEncoding = System.Text.Encoding.UTF8,
                };
                var process = Process.Start(processStart);
                if (process is null)
                    throw new InvalidOperationException("Failed to start process.");
                await using (var stdin = process.StandardInput)
                    await stdin.BaseStream.WriteAsync(logJsonBytes).ConfigureAwait(false);

                var resultStream = new MemoryStream();
                using (var stdout = process.StandardOutput)
                    await stdout.BaseStream.CopyToAsync(resultStream).ConfigureAwait(false);

                return resultStream.ToArray();
            }
            finally
            {
                _semaphore.Release();
            }
        });
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
#nullable restore