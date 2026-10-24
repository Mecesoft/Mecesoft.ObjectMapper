namespace Mecesoft.ObjectMapper.Tests;

/// <summary>
/// Runs an infinite sequence in a child process so a regression in collection-size
/// enforcement cannot hang or exhaust the main test process.
/// </summary>
internal static class InfiniteCollectionProbe
{
    public static int Run()
    {
        try
        {
            var mapper = new MapperConfiguration(
                cfg => cfg.CreateMap<EnumerableSource, IntListDestination>(),
                new MapperConfigurationOptions { MaxCollectionSize = 25 }).CreateMapper();

            mapper.Map<IntListDestination>(new EnumerableSource { Values = Infinite() });

            Console.Error.WriteLine("Infinite collection mapping completed unexpectedly.");
            return 2;
        }
        catch (MappingException exception)
        {
            if (exception.MemberPath == "Values[25]" &&
                exception.Message.Contains("MaxCollectionSize", StringComparison.Ordinal) &&
                exception.Message.Contains("25", StringComparison.Ordinal))
            {
                Console.WriteLine("Infinite collection probe completed successfully.");
                return 0;
            }

            Console.Error.WriteLine(exception);
            return 3;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static IEnumerable<int> Infinite()
    {
        var value = 0;

        while (true)
        {
            yield return value++;
        }
    }
}
