using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Mecesoft.ObjectMapper;

internal sealed class CollectionSourceInfo
{
    public CollectionSourceInfo(bool isSupported, Type? elementType)
    {
        IsSupported = isSupported;
        ElementType = elementType;
    }

    public bool IsSupported { get; }

    public Type? ElementType { get; }
}

internal sealed class CollectionDestinationInfo
{
    public CollectionDestinationInfo(
        bool isSupported,
        Type? elementType = null,
        bool isArray = false,
        Type? runtimeType = null,
        Func<int, object>? create = null,
        Action<object, object?>? add = null,
        Action<object, int, object?>? set = null)
    {
        IsSupported = isSupported;
        ElementType = elementType;
        IsArray = isArray;
        RuntimeType = runtimeType;
        Create = create;
        Add = add;
        Set = set;
    }

    public bool IsSupported { get; }

    public Type? ElementType { get; }

    public bool IsArray { get; }

    public Type? RuntimeType { get; }

    public Func<int, object>? Create { get; }

    public Action<object, object?>? Add { get; }

    public Action<object, int, object?>? Set { get; }
}

internal static class CollectionClassifier
{
    private static readonly ConcurrentDictionary<Type, bool> CollectionTypeCache = new();
    private static readonly ConcurrentDictionary<Type, bool> DictionaryTypeCache = new();
    private static readonly ConcurrentDictionary<Type, CollectionSourceInfo> SourceInfoCache = new();
    private static readonly ConcurrentDictionary<Type, CollectionDestinationInfo> DestinationInfoCache = new();

    public static bool IsCollection(Type type)
    {
        return CollectionTypeCache.GetOrAdd(
            type,
            static candidate => candidate != typeof(string) &&
                                typeof(IEnumerable).IsAssignableFrom(candidate));
    }

    public static bool IsDictionary(Type type)
    {
        return DictionaryTypeCache.GetOrAdd(type, static candidate => CreateDictionaryInfo(candidate));
    }

    public static CollectionSourceInfo GetSourceInfo(Type type)
    {
        return SourceInfoCache.GetOrAdd(type, static candidate => CreateSourceInfo(candidate));
    }

    public static CollectionDestinationInfo GetDestinationInfo(Type type)
    {
        return DestinationInfoCache.GetOrAdd(type, static candidate => CreateDestinationInfo(candidate));
    }

    private static bool CreateDictionaryInfo(Type type)
    {
        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            return true;
        }

        if (IsGenericDictionaryType(type))
        {
            return true;
        }

        foreach (var interfaceType in type.GetInterfaces())
        {
            if (IsGenericDictionaryType(interfaceType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsGenericDictionaryType(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        var definition = type.GetGenericTypeDefinition();
        return definition == typeof(IDictionary<,>) ||
               definition == typeof(IReadOnlyDictionary<,>);
    }

    private static CollectionSourceInfo CreateSourceInfo(Type type)
    {
        if (type == typeof(string))
        {
            return new CollectionSourceInfo(false, null);
        }

        if (type.IsArray)
        {
            return type.GetArrayRank() == 1
                ? new CollectionSourceInfo(true, type.GetElementType())
                : new CollectionSourceInfo(false, null);
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return new CollectionSourceInfo(true, type.GetGenericArguments()[0]);
        }

        Type? elementType = null;
        foreach (var interfaceType in type.GetInterfaces())
        {
            if (!interfaceType.IsGenericType ||
                interfaceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
            {
                continue;
            }

            var candidate = interfaceType.GetGenericArguments()[0];
            if (elementType is null)
            {
                elementType = candidate;
            }
            else if (elementType != candidate)
            {
                return new CollectionSourceInfo(false, null);
            }
        }

        return elementType is null
            ? new CollectionSourceInfo(false, null)
            : new CollectionSourceInfo(true, elementType);
    }

    private static CollectionDestinationInfo CreateDestinationInfo(Type type)
    {
        if (type.IsArray)
        {
            if (type.GetArrayRank() != 1)
            {
                return new CollectionDestinationInfo(false);
            }

            var elementType = type.GetElementType()!;
            return new CollectionDestinationInfo(
                true,
                elementType,
                isArray: true,
                runtimeType: type,
                create: CreateArrayFactory(elementType),
                set: CreateArraySetter(type, elementType));
        }

        if (!type.IsGenericType)
        {
            return new CollectionDestinationInfo(false);
        }

        var definition = type.GetGenericTypeDefinition();
        if (definition != typeof(List<>) &&
            definition != typeof(IEnumerable<>) &&
            definition != typeof(ICollection<>) &&
            definition != typeof(IList<>) &&
            definition != typeof(IReadOnlyCollection<>) &&
            definition != typeof(IReadOnlyList<>))
        {
            return new CollectionDestinationInfo(false);
        }

        var targetElementType = type.GetGenericArguments()[0];
        var listType = typeof(List<>).MakeGenericType(targetElementType);

        return new CollectionDestinationInfo(
            true,
            targetElementType,
            isArray: false,
            runtimeType: listType,
            create: CreateListFactory(listType),
            add: CreateListAdd(listType, targetElementType));
    }

    private static Func<int, object> CreateListFactory(Type listType)
    {
        try
        {
            var capacity = Expression.Parameter(typeof(int), "capacity");
            var constructor = listType.GetConstructor(new[] { typeof(int) })!;
            var body = Expression.Convert(
                Expression.New(constructor, capacity),
                typeof(object));

            return Expression.Lambda<Func<int, object>>(body, capacity).Compile();
        }
        catch (Exception exception) when (!ExceptionHelper.IsFatal(exception))
        {
            return capacity => Activator.CreateInstance(listType, capacity)!;
        }
    }

    private static Action<object, object?> CreateListAdd(Type listType, Type elementType)
    {
        try
        {
            var list = Expression.Parameter(typeof(object), "list");
            var value = Expression.Parameter(typeof(object), "value");
            var addMethod = listType.GetMethod("Add", new[] { elementType })!;
            var call = Expression.Call(
                Expression.Convert(list, listType),
                addMethod,
                Expression.Convert(value, elementType));

            return Expression.Lambda<Action<object, object?>>(call, list, value).Compile();
        }
        catch (Exception exception) when (!ExceptionHelper.IsFatal(exception))
        {
            return (list, value) => ((IList)list).Add(value);
        }
    }

    private static Func<int, object> CreateArrayFactory(Type elementType)
    {
        try
        {
            var count = Expression.Parameter(typeof(int), "count");
            var body = Expression.Convert(
                Expression.NewArrayBounds(elementType, count),
                typeof(object));

            return Expression.Lambda<Func<int, object>>(body, count).Compile();
        }
        catch (Exception exception) when (!ExceptionHelper.IsFatal(exception))
        {
            return count => Array.CreateInstance(elementType, count);
        }
    }

    private static Action<object, int, object?> CreateArraySetter(Type arrayType, Type elementType)
    {
        try
        {
            var array = Expression.Parameter(typeof(object), "array");
            var index = Expression.Parameter(typeof(int), "index");
            var value = Expression.Parameter(typeof(object), "value");
            var assignment = Expression.Assign(
                Expression.ArrayAccess(
                    Expression.Convert(array, arrayType),
                    index),
                Expression.Convert(value, elementType));

            return Expression.Lambda<Action<object, int, object?>>(
                assignment,
                array,
                index,
                value).Compile();
        }
        catch (Exception exception) when (!ExceptionHelper.IsFatal(exception))
        {
            return (array, index, value) => ((IList)array)[index] = value;
        }
    }
}
