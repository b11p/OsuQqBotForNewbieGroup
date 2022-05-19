using System;
using System.IO;
using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.Mahjong;

#nullable enable
public class MahjongObjectStorage
{
    private readonly string _basePath;
    private readonly Uri _baseUrl;

    public MahjongObjectStorage(string basePath, string baseUrl)
    {
        _basePath = basePath;
        _baseUrl = new Uri(baseUrl);
    }

    public async Task<Uri?> PutFileAsync(string id, ReadOnlyMemory<byte> bytes, bool overwrite = false)
    {
        var path = Path.Combine(_basePath, id);
        if (File.Exists(path) && !overwrite)
            return null;

        await using var fileStream = File.Create(path);
        await fileStream.WriteAsync(bytes).ConfigureAwait(false);
        return new Uri(_baseUrl, id);
    }

    public Task<Uri?> GetUriAsync(string id)
    {
        var path = Path.Combine(_basePath, id);
        if (!File.Exists(path))
            return Task.FromResult<Uri?>(null);

        return Task.FromResult<Uri?>(new Uri(_baseUrl, id));
    }
}
#nullable restore