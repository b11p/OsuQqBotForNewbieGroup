namespace Bleatingsheep.NewHydrant
{
    public interface IConfigure
    {
        string ApiKey { get; }
        long SuperAdmin { get; }
        string Listen { get; }
        string ApiAddress { get; }
        string Name { get; }
    }
}
