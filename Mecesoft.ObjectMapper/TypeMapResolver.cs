namespace Mecesoft.ObjectMapper;

internal static class TypeMapResolver
{
    public static Type? ResolveSourceType(
        IEnumerable<(Type SourceType, Type DestinationType)> typeMaps,
        Type runtimeSourceType,
        Type destinationType)
    {
        List<Type>? candidates = null;

        foreach (var typeMap in typeMaps)
        {
            if (typeMap.DestinationType != destinationType ||
                !typeMap.SourceType.IsAssignableFrom(runtimeSourceType))
            {
                continue;
            }

            candidates ??= new List<Type>();
            if (!candidates.Contains(typeMap.SourceType))
            {
                candidates.Add(typeMap.SourceType);
            }
        }

        if (candidates is null)
        {
            return null;
        }

        if (candidates.Count == 1)
        {
            return candidates[0];
        }

        var bestDistance = int.MaxValue;
        var bestClasses = new List<Type>();
        var bestInterfaces = new List<Type>();

        foreach (var candidate in candidates)
        {
            var distance = GetDistance(runtimeSourceType, candidate);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestClasses.Clear();
                bestInterfaces.Clear();
            }

            if (distance != bestDistance)
            {
                continue;
            }

            if (candidate.IsInterface)
            {
                bestInterfaces.Add(candidate);
            }
            else
            {
                bestClasses.Add(candidate);
            }
        }

        if (bestClasses.Count == 1)
        {
            return bestClasses[0];
        }

        if (bestClasses.Count == 0 && bestInterfaces.Count == 1)
        {
            return bestInterfaces[0];
        }

        throw CreateAmbiguityException(runtimeSourceType, destinationType, candidates);
    }

    private static int GetDistance(Type runtimeSourceType, Type candidate)
    {
        if (candidate == runtimeSourceType)
        {
            return 0;
        }

        var visited = new HashSet<Type>();
        var queue = new Queue<(Type Type, int Distance)>();
        queue.Enqueue((runtimeSourceType, 0));

        while (queue.Count > 0)
        {
            var (current, distance) = queue.Dequeue();

            if (!visited.Add(current))
            {
                continue;
            }

            if (current == candidate)
            {
                return distance;
            }

            if (current.BaseType is not null)
            {
                queue.Enqueue((current.BaseType, distance + 1));
            }

            foreach (var interfaceType in current.GetInterfaces())
            {
                queue.Enqueue((interfaceType, distance + 1));
            }
        }

        return int.MaxValue;
    }

    private static MappingConfigurationException CreateAmbiguityException(
        Type runtimeSourceType,
        Type destinationType,
        IEnumerable<Type> candidates)
    {
        var candidateNames = candidates
            .Select(candidate => candidate.FullName)
            .OrderBy(name => name, StringComparer.Ordinal);

        return new MappingConfigurationException(
            $"Ambiguous mapping from '{runtimeSourceType.FullName}' to " +
            $"'{destinationType.FullName}'. Candidates: {string.Join(", ", candidateNames)}. " +
            "Define an explicit map for the runtime source type or remove ambiguous base/interface maps.");
    }
}
