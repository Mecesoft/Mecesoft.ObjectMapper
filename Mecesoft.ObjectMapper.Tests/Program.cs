namespace Mecesoft.ObjectMapper.Tests;

public static class Program
{
    private const string CircularReferenceProbeArgument = "--circular-reference-probe";
    private const string InfiniteCollectionProbeArgument = "--infinite-collection-probe";

    public static int Main(string[] args)
    {
        if (args is [CircularReferenceProbeArgument])
        {
            return CircularReferenceProbe.Run();
        }

        if (args is [InfiniteCollectionProbeArgument])
        {
            return InfiniteCollectionProbe.Run();
        }

        return 0;
    }
}
