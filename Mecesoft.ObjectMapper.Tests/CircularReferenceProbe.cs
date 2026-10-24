namespace Mecesoft.ObjectMapper.Tests;

/// <summary>
/// Safety probe: runs circular mapping in a child process so that a future regression to
/// unbounded recursion cannot take down the main test process.
/// </summary>
internal static class CircularReferenceProbe
{
    public static int Run()
    {
        try
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<CircularNode, CircularNodeDto>());
            var mapper = configuration.CreateMapper();

            var first = new CircularNode { Name = "first" };
            var second = new CircularNode { Name = "second", Next = first };
            first.Next = second;

            var destination = mapper.Map<CircularNodeDto>(first);

            if (destination.Next is null ||
                !ReferenceEquals(destination.Next.Next, destination))
            {
                Console.Error.WriteLine("Circular reference was not preserved.");
                return 2;
            }

            Console.WriteLine("Circular reference probe completed successfully.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }
}
