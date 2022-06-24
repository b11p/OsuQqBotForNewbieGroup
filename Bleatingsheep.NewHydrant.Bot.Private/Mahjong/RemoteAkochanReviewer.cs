using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.Mahjong;

#nullable enable
public class RemoteAkochanReviewer : IMajsoulAnalyzer, IDisposable
{
    private bool _disposedValue;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly HttpClient _httpClient = new HttpClient();

    public bool IsIdle => _semaphore.CurrentCount == 1;

    public RemoteAkochanReviewer()
    {

    }

    public async Task<byte[]> AnalyzeAsync(ReadOnlyMemory<byte> logJsonBytes, int targetActor, int[] ptList, double deviationThreshold, string id)
    {
        if (!_semaphore.Wait(0))
        {
            throw new InvalidOperationException("An analysis is being processed.");
        }

        try
        {
            var request = new
            {
                id = id,
                data = Convert.ToBase64String(logJsonBytes.Span),
                targetActor = targetActor,
                ptList = ptList,
                extraPer1000 = 0.0,
                deviationThreshold = deviationThreshold,
            };
            var response = await _httpClient.PostAsJsonAsync("http://akochan/start", request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            while (true)
            {
                await Task.Delay(10000).ConfigureAwait(false);
                try
                {
                    var state = await _httpClient.GetFromJsonAsync<RunningState>("http://akochan/status").ConfigureAwait(false);
                    if (state?.IsRunning == false)
                    {
                        break;
                    }
                }
                catch
                {
                    // ignored
                }
            }
            var result = await _httpClient.GetByteArrayAsync($"https://tx.b11p.com:1443/{id}.html").ConfigureAwait(false);
            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    ~RemoteAkochanReviewer()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private sealed class RunningState
    {
        [JsonPropertyName("isRunning")]
        public bool IsRunning { get; set; }
    }
}
#nullable restore