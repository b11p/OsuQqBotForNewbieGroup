using System;
using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.Mahjong;

#nullable enable
public interface IMajsoulAnalyzer
{
    Task<byte[]> AnalyzeAsync(ReadOnlyMemory<byte> logJsonBytes, int targetActor, int[] ptList, double deviationThreshold, string id);

    bool IsIdle { get; }
}
#nullable restore