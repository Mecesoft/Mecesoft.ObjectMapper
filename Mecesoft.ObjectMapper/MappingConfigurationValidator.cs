using System.Reflection;

namespace Mecesoft.ObjectMapper;

internal static class MappingConfigurationValidator
{
    public static void AssertConfigurationIsValid(IReadOnlyList<TypeMap> typeMaps)
    {
        var errors = new List<string>();
        var typePairs = new (Type SourceType, Type DestinationType)[typeMaps.Count];

        for (var index = 0; index < typeMaps.Count; index++)
        {
            typePairs[index] = (typeMaps[index].SourceType, typeMaps[index].DestinationType);
        }

        foreach (var typeMap in typeMaps)
        {
            var mapName =
                $"Map '{GetTypeName(typeMap.SourceType)}' -> '{GetTypeName(typeMap.DestinationType)}'" +
                (typeMap.IsReverseMap ? " (reverse)" : string.Empty);
            var destinationProperties = typeMap.DestinationType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            foreach (var destinationProperty in destinationProperties)
            {
                if (!destinationProperty.CanWrite ||
                    destinationProperty.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                typeMap.TryGetMemberMap(destinationProperty.Name, out var memberMap);

                if (memberMap is not null &&
                    (memberMap.Ignore || memberMap.MapFrom is not null))
                {
                    continue;
                }

                var sourceProperty = GetReadableProperty(
                    typeMap.SourceType,
                    destinationProperty.Name);

                if (sourceProperty is null)
                {
                    errors.Add(
                        $"{mapName}, member '{destinationProperty.Name}': destination member " +
                        "has no matching source member. Use Ignore or MapFrom to make this intentional.");
                    continue;
                }

                ValidateTypeMapping(
                    sourceProperty.PropertyType,
                    destinationProperty.PropertyType,
                    $"{mapName}, member '{destinationProperty.Name}'",
                    typePairs,
                    errors,
                    new HashSet<(Type SourceType, Type DestinationType)>());
            }
        }

        if (errors.Count > 0)
        {
            throw new MappingConfigurationException(
                "Mapping configuration is invalid:" +
                Environment.NewLine +
                " - " +
                string.Join(Environment.NewLine + " - ", errors));
        }
    }

    private static bool ValidateTypeMapping(
        Type sourceType,
        Type destinationType,
        string path,
        IReadOnlyList<(Type SourceType, Type DestinationType)> typePairs,
        List<string> errors,
        HashSet<(Type SourceType, Type DestinationType)> visited)
    {
        if (!visited.Add((sourceType, destinationType)))
        {
            return true;
        }

        if (CanAssignOrConvert(sourceType, destinationType))
        {
            return true;
        }

        if (CollectionClassifier.IsDictionary(sourceType) ||
            CollectionClassifier.IsDictionary(destinationType))
        {
            errors.Add($"{path}[]: dictionary mapping is not supported.");
            return false;
        }

        var sourceIsCollection = CollectionClassifier.IsCollection(sourceType);
        var destinationIsCollection = CollectionClassifier.IsCollection(destinationType);

        if (destinationIsCollection)
        {
            var destinationInfo = CollectionClassifier.GetDestinationInfo(destinationType);

            if (!destinationInfo.IsSupported)
            {
                errors.Add(
                    $"{path}: collection destination type '{destinationType.FullName}' is not supported.");
                return false;
            }

            if (sourceIsCollection)
            {
                var sourceInfo = CollectionClassifier.GetSourceInfo(sourceType);

                if (!sourceInfo.IsSupported)
                {
                    errors.Add(
                        $"{path}: collection source type '{sourceType.FullName}' is not a supported " +
                        "generic collection.");
                    return false;
                }

                return ValidateTypeMapping(
                    sourceInfo.ElementType!,
                    destinationInfo.ElementType!,
                    $"{path}[]",
                    typePairs,
                    errors,
                    visited);
            }
        }
        else if (sourceIsCollection)
        {
            if (TryResolveMap(typePairs, sourceType, destinationType, path, errors))
            {
                return true;
            }

            errors.Add(
                $"{path}: collection source type '{sourceType.FullName}' cannot be mapped to " +
                $"non-collection type '{destinationType.FullName}'.");
            return false;
        }

        if (TryResolveMap(typePairs, sourceType, destinationType, path, errors))
        {
            return true;
        }

        errors.Add(
            $"{path}: no conversion or nested map is available from " +
            $"'{sourceType.FullName}' to '{destinationType.FullName}'.");
        return false;
    }

    private static bool TryResolveMap(
        IReadOnlyList<(Type SourceType, Type DestinationType)> typePairs,
        Type sourceType,
        Type destinationType,
        string path,
        List<string> errors)
    {
        try
        {
            return TypeMapResolver.ResolveSourceType(typePairs, sourceType, destinationType) is not null;
        }
        catch (MappingConfigurationException exception)
        {
            errors.Add($"{path}: {exception.Message}");
            return true;
        }
    }

    private static bool CanAssignOrConvert(Type sourceType, Type destinationType)
    {
        if (destinationType.IsAssignableFrom(sourceType))
        {
            return true;
        }

        var source = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
        var destination = Nullable.GetUnderlyingType(destinationType) ?? destinationType;

        if (source == destination)
        {
            return true;
        }

        if (destination == typeof(Guid) && source == typeof(string))
        {
            return true;
        }

        if (destination == typeof(string) && source == typeof(Guid))
        {
            return true;
        }

        if (destination.IsEnum &&
            (source == typeof(string) || IsNumeric(source)))
        {
            return true;
        }

        if (typeof(IConvertible).IsAssignableFrom(source) &&
            IsConvertibleDestination(destination))
        {
            return true;
        }

        return false;
    }

    private static bool IsConvertibleDestination(Type type)
    {
        return type.IsPrimitive ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime);
    }

    private static bool IsNumeric(Type type)
    {
        return type == typeof(byte) || type == typeof(sbyte) ||
               type == typeof(short) || type == typeof(ushort) ||
               type == typeof(int) || type == typeof(uint) ||
               type == typeof(long) || type == typeof(ulong) ||
               type == typeof(float) || type == typeof(double) ||
               type == typeof(decimal);
    }

    private static PropertyInfo? GetReadableProperty(Type sourceType, string memberName)
    {
        var properties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.Name == memberName &&
                property.CanRead &&
                property.GetIndexParameters().Length == 0)
            {
                return property;
            }
        }

        return null;
    }

    private static string GetTypeName(Type type)
    {
        return type.FullName ?? type.Name;
    }
}
