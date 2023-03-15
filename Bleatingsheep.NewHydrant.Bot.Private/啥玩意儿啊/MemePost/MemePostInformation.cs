namespace Bleatingsheep.NewHydrant.啥玩意儿啊.MemePost;
#nullable enable
internal class MemePostInformation
{
    public required MemePostRepositoryInformation Repository { get; init; }
    public required string GitHubToken { get; init; }
    public required string Path { get; init; }

    internal record class MemePostRepositoryInformation(string Owner, string Name);
}
