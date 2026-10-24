using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mecesoft.ObjectMapper;

internal sealed class Mapper : IMapper
{
    private readonly MappingPlanCache _cache;
    private readonly int _maxDepth;
    private readonly int _maxCollectionSize;

    public Mapper(MappingPlanCache cache, int maxDepth, int maxCollectionSize)
    {
        _cache = cache;
        _maxDepth = maxDepth;
        _maxCollectionSize = maxCollectionSize;
    }

    public TDestination Map<TDestination>(object source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return (TDestination)Map(source, typeof(TDestination));
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return (TDestination)Map(source, typeof(TDestination));
    }

    public object Map(object source, Type destinationType)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (destinationType is null)
        {
            throw new ArgumentNullException(nameof(destinationType));
        }

        var sourceType = source.GetType();
        var context = new MappingContext();

        if (CollectionClassifier.IsCollection(sourceType) ||
            CollectionClassifier.IsCollection(destinationType))
        {
            return MapValue(
                source,
                destinationType,
                sourceType,
                destinationType,
                context);
        }

        return MapInternal(source, sourceType, destinationType, context);
    }

    private object MapInternal(
        object source,
        Type sourceType,
        Type destinationType,
        MappingContext context)
    {
        if (destinationType.IsAssignableFrom(sourceType))
        {
            return source;
        }

        if (!_cache.TryGetTypeMap(sourceType, destinationType, out var typeMap))
        {
            throw new InvalidOperationException(
                $"No map configured from '{sourceType.Name}' to '{destinationType.Name}'.");
        }

        return MapInternal(source, sourceType, destinationType, typeMap, context);
    }

    private object MapInternal(
        object source,
        Type sourceType,
        Type destinationType,
        TypeMapSnapshot typeMap,
        MappingContext context)
    {
        if (context.TryGet(source, destinationType, out var existingDestination))
        {
            return existingDestination;
        }

        var plan = _cache.GetPlan(typeMap);
        context.EnterDepth(_maxDepth, sourceType, destinationType);

        try
        {
            object destination;

            try
            {
                destination = plan.CreateDestination();
            }
            catch (Exception exception) when (
                ExceptionHelper.ShouldWrap(exception) &&
                exception is not MissingMethodException)
            {
                throw CreateFactoryException(sourceType, destinationType, context, exception);
            }

            if (!sourceType.IsValueType && !destinationType.IsValueType)
            {
                context.Register(source, destinationType, destination);
            }

            foreach (var member in plan.Members)
            {
                context.PushMember(member.Name);

                try
                {
                    object? value;

                    if (member.MapFrom is not null)
                    {
                        value = GetMappedValue(
                            member.MapFrom,
                            source,
                            sourceType,
                            destinationType,
                            context);
                    }
                    else
                    {
                        try
                        {
                            value = member.SourceGetter!(source);
                        }
                        catch (Exception exception) when (ExceptionHelper.ShouldWrap(exception))
                        {
                            throw CreateSourceAccessException(
                                sourceType,
                                destinationType,
                                context,
                                exception);
                        }
                    }

                    AssignValue(
                        destination,
                        member,
                        value,
                        sourceType,
                        destinationType,
                        context);
                }
                finally
                {
                    context.Pop();
                }
            }

            return destination;
        }
        finally
        {
            context.ExitDepth();
        }
    }

    private static object? GetMappedValue(
        Func<object, object?> mapFrom,
        object source,
        Type sourceType,
        Type destinationType,
        MappingContext context)
    {
        try
        {
            return mapFrom(source);
        }
        catch (Exception exception) when (ExceptionHelper.ShouldWrap(exception))
        {
            var path = context.RenderPath();
            throw new MappingException(
                sourceType,
                destinationType,
                path,
                $"Failed to map member '{path}' from '{sourceType.FullName}' to " +
                $"'{destinationType.FullName}': the configured MapFrom expression threw an exception.",
                exception);
        }
    }

    private void AssignValue(
        object destination,
        MemberPlan member,
        object? value,
        Type sourceType,
        Type destinationType,
        MappingContext context)
    {
        if (value is null)
        {
            if (IsNullable(member.PropertyType))
            {
                try
                {
                    member.Setter(destination, null);
                }
                catch (Exception exception) when (ExceptionHelper.ShouldWrap(exception))
                {
                    throw CreateDestinationAccessException(
                        sourceType,
                        destinationType,
                        context,
                        exception);
                }
            }

            return;
        }

        var mappedValue = MapValue(
            value,
            member.PropertyType,
            sourceType,
            destinationType,
            context);

        try
        {
            member.Setter(destination, mappedValue);
        }
        catch (Exception exception) when (ExceptionHelper.ShouldWrap(exception))
        {
            throw CreateDestinationAccessException(
                sourceType,
                destinationType,
                context,
                exception);
        }
    }

    private object MapValue(
        object value,
        Type targetType,
        Type sourceContextType,
        Type destinationContextType,
        MappingContext context)
    {
        var valueType = value.GetType();

        if (CollectionClassifier.IsCollection(targetType))
        {
            if (CollectionClassifier.IsDictionary(valueType) ||
                CollectionClassifier.IsDictionary(targetType))
            {
                throw CreateDictionaryMappingException(
                    sourceContextType,
                    destinationContextType,
                    context.RenderPath(),
                    valueType,
                    targetType);
            }

            var destinationInfo = CollectionClassifier.GetDestinationInfo(targetType);
            if (destinationInfo.IsSupported)
            {
                var sourceInfo = CollectionClassifier.GetSourceInfo(valueType);
                if (sourceInfo.IsSupported)
                {
                    return MapCollection(
                        value,
                        sourceInfo.ElementType!,
                        destinationInfo,
                        sourceContextType,
                        destinationContextType,
                        context);
                }
            }
            else
            {
                throw CreateUnsupportedCollectionException(
                    sourceContextType,
                    destinationContextType,
                    context.RenderPath(),
                    valueType,
                    targetType,
                    $"collection destination type '{targetType.FullName}' is not supported. " +
                    "Supported destinations are arrays, List<T>, IEnumerable<T>, ICollection<T>, " +
                    "IList<T>, IReadOnlyCollection<T> and IReadOnlyList<T>.");
            }
        }

        if (targetType.IsAssignableFrom(valueType))
        {
            return value;
        }

        object? convertedValue = null;
        Exception? conversionException = null;

        try
        {
            if (TryConvert(value, targetType, out convertedValue))
            {
                return convertedValue!;
            }
        }
        catch (Exception exception) when (ExceptionHelper.ShouldWrap(exception))
        {
            conversionException = exception;
        }

        if (_cache.TryGetTypeMap(valueType, targetType, out var nestedMap))
        {
            return MapInternal(value, valueType, targetType, nestedMap, context);
        }

        if (CollectionClassifier.IsCollection(targetType))
        {
            throw CreateUnsupportedCollectionException(
                sourceContextType,
                destinationContextType,
                context.RenderPath(),
                valueType,
                targetType,
                CreateCollectionTargetReason(valueType, targetType));
        }

        if (CollectionClassifier.IsCollection(valueType))
        {
            throw CreateUnsupportedCollectionException(
                sourceContextType,
                destinationContextType,
                context.RenderPath(),
                valueType,
                targetType,
                $"collection value of type '{valueType.FullName}' cannot be mapped to " +
                $"non-collection type '{targetType.FullName}'.");
        }

        throw CreateMappingException(
            sourceContextType,
            destinationContextType,
            context.RenderPath(),
            valueType,
            targetType,
            conversionException);
    }

    private object MapCollection(
        object sourceCollection,
        Type sourceElementType,
        CollectionDestinationInfo destinationInfo,
        Type sourceContextType,
        Type destinationContextType,
        MappingContext context)
    {
        var trackingType = destinationInfo.RuntimeType!;

        if (context.TryGet(sourceCollection, trackingType, out var existingDestination))
        {
            return existingDestination;
        }

        var knownCount = GetKnownCount(sourceCollection);
        if (knownCount > _maxCollectionSize)
        {
            throw CreateCollectionSizeException(
                sourceContextType,
                destinationContextType,
                context,
                _maxCollectionSize,
                knownCount,
                knownCount: true);
        }

        context.EnterDepth(_maxDepth, sourceCollection.GetType(), trackingType);

        try
        {
            return destinationInfo.IsArray
                ? MapArrayCollection(
                    sourceCollection,
                    sourceElementType,
                    destinationInfo,
                    trackingType,
                    knownCount,
                    sourceContextType,
                    destinationContextType,
                    context)
                : MapListCollection(
                    sourceCollection,
                    sourceElementType,
                    destinationInfo,
                    trackingType,
                    knownCount,
                    sourceContextType,
                    destinationContextType,
                    context);
        }
        finally
        {
            context.ExitDepth();
        }
    }

    private object MapListCollection(
        object sourceCollection,
        Type sourceElementType,
        CollectionDestinationInfo destinationInfo,
        Type trackingType,
        int knownCount,
        Type sourceContextType,
        Type destinationContextType,
        MappingContext context)
    {
        var destinationList = destinationInfo.Create!(knownCount >= 0 ? knownCount : 0);
        context.Register(sourceCollection, trackingType, destinationList);

        var enumerator = GetEnumerator(
            sourceCollection,
            sourceContextType,
            destinationContextType,
            context);

        Exception? pendingException = null;

        try
        {
            var index = 0;

            while (true)
            {
                context.PushIndex(index);

                try
                {
                    if (!TryReadNext(
                            enumerator,
                            sourceContextType,
                            destinationContextType,
                            context,
                            index,
                            _maxCollectionSize,
                            out var item))
                    {
                        break;
                    }

                    var mappedElement = MapElement(
                        item,
                        sourceElementType,
                        destinationInfo.ElementType!,
                        sourceContextType,
                        destinationContextType,
                        context);

                    try
                    {
                        destinationInfo.Add!(destinationList, mappedElement);
                    }
                    catch (Exception exception) when (ExceptionHelper.ShouldWrap(exception))
                    {
                        throw CreateCollectionWriteException(
                            sourceContextType,
                            destinationContextType,
                            context,
                            index,
                            exception);
                    }
                }
                finally
                {
                    context.Pop();
                }

                index++;
            }
        }
        catch (Exception exception)
        {
            pendingException = exception;
            throw;
        }
        finally
        {
            DisposeEnumerator(
                enumerator,
                pendingException,
                sourceContextType,
                destinationContextType,
                context);
        }

        return destinationList;
    }

    private object MapArrayCollection(
        object sourceCollection,
        Type sourceElementType,
        CollectionDestinationInfo destinationInfo,
        Type trackingType,
        int knownCount,
        Type sourceContextType,
        Type destinationContextType,
        MappingContext context)
    {
        if (knownCount >= 0)
        {
            var destinationArray = (Array)destinationInfo.Create!(knownCount);
            context.Register(sourceCollection, trackingType, destinationArray);

            var enumerator = GetEnumerator(
                sourceCollection,
                sourceContextType,
                destinationContextType,
                context);

            Exception? pendingException = null;

            try
            {
                var index = 0;

                while (true)
                {
                    context.PushIndex(index);

                    try
                    {
                        if (!TryReadNext(
                                enumerator,
                                sourceContextType,
                                destinationContextType,
                                context,
                                index,
                                _maxCollectionSize,
                                out var item))
                        {
                            break;
                        }

                        var mappedElement = MapElement(
                            item,
                            sourceElementType,
                            destinationInfo.ElementType!,
                            sourceContextType,
                            destinationContextType,
                            context);

                        try
                        {
                            destinationInfo.Set!(destinationArray, index, mappedElement);
                        }
                        catch (Exception exception) when (ExceptionHelper.ShouldWrap(exception))
                        {
                            throw CreateCollectionWriteException(
                                sourceContextType,
                                destinationContextType,
                                context,
                                index,
                                exception);
                        }
                    }
                    finally
                    {
                        context.Pop();
                    }

                    index++;
                }
            }
            catch (Exception exception)
            {
                pendingException = exception;
                throw;
            }
            finally
            {
                DisposeEnumerator(
                    enumerator,
                    pendingException,
                    sourceContextType,
                    destinationContextType,
                    context);
            }

            return destinationArray;
        }

        var sourceItems = MaterializeCollection(
            sourceCollection,
            sourceContextType,
            destinationContextType,
            context,
            _maxCollectionSize);

        var array = (Array)destinationInfo.Create!(sourceItems.Count);
        context.Register(sourceCollection, trackingType, array);

        for (var index = 0; index < sourceItems.Count; index++)
        {
            context.PushIndex(index);

            try
            {
                var mappedElement = MapElement(
                    sourceItems[index],
                    sourceElementType,
                    destinationInfo.ElementType!,
                    sourceContextType,
                    destinationContextType,
                    context);

                try
                {
                    destinationInfo.Set!(array, index, mappedElement);
                }
                catch (Exception exception) when (ExceptionHelper.ShouldWrap(exception))
                {
                    throw CreateCollectionWriteException(
                        sourceContextType,
                        destinationContextType,
                        context,
                        index,
                        exception);
                }
            }
            finally
            {
                context.Pop();
            }
        }

        return array;
    }

    private static List<object?> MaterializeCollection(
        object sourceCollection,
        Type sourceContextType,
        Type destinationContextType,
        MappingContext context,
        int maxCollectionSize)
    {
        var items = new List<object?>();
        var enumerator = GetEnumerator(
            sourceCollection,
            sourceContextType,
            destinationContextType,
            context);

        Exception? pendingException = null;

        try
        {
            var index = 0;

            while (true)
            {
                context.PushIndex(index);

                try
                {
                    if (!TryReadNext(
                            enumerator,
                            sourceContextType,
                            destinationContextType,
                            context,
                            index,
                            maxCollectionSize,
                            out var item))
                    {
                        break;
                    }

                    items.Add(item);
                }
                finally
                {
                    context.Pop();
                }

                index++;
            }
        }
        catch (Exception exception)
        {
            pendingException = exception;
            throw;
        }
        finally
        {
            DisposeEnumerator(
                enumerator,
                pendingException,
                sourceContextType,
                destinationContextType,
                context);
        }

        return items;
    }

    private static IEnumerator GetEnumerator(
        object sourceCollection,
        Type sourceContextType,
        Type destinationContextType,
        MappingContext context)
    {
        try
        {
            return ((IEnumerable)sourceCollection).GetEnumerator();
        }
        catch (Exception exception) when (ExceptionHelper.ShouldWrap(exception))
        {
            var path = context.RenderPath();
            throw new MappingException(
                sourceContextType,
                destinationContextType,
                path,
                $"Failed to get an enumerator for collection member '{path}': {exception.Message}",
                exception);
        }
    }

    private static bool TryReadNext(
        IEnumerator enumerator,
        Type sourceContextType,
        Type destinationContextType,
        MappingContext context,
        int index,
        int maxCollectionSize,
        out object? item)
    {
        try
        {
            if (!enumerator.MoveNext())
            {
                item = null;
                return false;
            }

            if (index >= maxCollectionSize)
            {
                item = null;
                throw CreateCollectionSizeException(
                    sourceContextType,
                    destinationContextType,
                    context,
                    maxCollectionSize,
                    index + 1,
                    knownCount: false);
            }

            item = enumerator.Current;
            return true;
        }
        catch (Exception exception) when (ExceptionHelper.ShouldWrap(exception))
        {
            item = null;
            throw CreateCollectionEnumerationException(
                sourceContextType,
                destinationContextType,
                context,
                index,
                exception);
        }
    }

    private object? MapElement(
        object? value,
        Type sourceElementType,
        Type targetElementType,
        Type sourceContextType,
        Type destinationContextType,
        MappingContext context)
    {
        if (value is null)
        {
            if (IsNullable(targetElementType))
            {
                return null;
            }

            var path = context.RenderPath();
            throw new MappingException(
                sourceContextType,
                destinationContextType,
                path,
                $"Failed to map member '{path}' from '{sourceContextType.FullName}' to " +
                $"'{destinationContextType.FullName}': null collection element of type " +
                $"'{sourceElementType.FullName}' cannot be mapped to non-nullable type " +
                $"'{targetElementType.FullName}'.");
        }

        return MapValue(
            value,
            targetElementType,
            sourceContextType,
            destinationContextType,
            context);
    }

    private static int GetKnownCount(object sourceCollection)
    {
        if (sourceCollection is Array array)
        {
            return array.Length;
        }

        if (sourceCollection is ICollection collection)
        {
            return collection.Count;
        }

        return -1;
    }

    private static void DisposeEnumerator(
        IEnumerator enumerator,
        Exception? pendingException,
        Type sourceType,
        Type destinationType,
        MappingContext context)
    {
        if (enumerator is not IDisposable disposable)
        {
            return;
        }

        try
        {
            disposable.Dispose();
        }
        catch (Exception exception) when (ExceptionHelper.ShouldWrap(exception))
        {
            if (pendingException is not null)
            {
                pendingException.Data["CollectionDisposeException"] = exception;
                return;
            }

            var path = context.RenderPath();
            throw new MappingException(
                sourceType,
                destinationType,
                path,
                $"Failed to dispose collection enumerator for '{path}': {exception.Message}",
                exception);
        }
    }

    private static MappingException CreateCollectionSizeException(
        Type sourceType,
        Type destinationType,
        MappingContext context,
        int limit,
        int observedCount,
        bool knownCount)
    {
        var path = context.RenderPath();
        var detail = knownCount
            ? $"contains {observedCount} elements"
            : $"has more than {limit} elements";

        return new MappingException(
            sourceType,
            destinationType,
            path,
            $"Collection '{path}' exceeds MaxCollectionSize of {limit} ({detail}). " +
            "Increase MapperConfigurationOptions.MaxCollectionSize or bound the source collection.");
    }

    private static MappingException CreateFactoryException(
        Type sourceType,
        Type destinationType,
        MappingContext context,
        Exception innerException)
    {
        var path = context.RenderPath();
        return new MappingException(
            sourceType,
            destinationType,
            path,
            $"Failed to map member '{path}' from '{sourceType.FullName}' to " +
            $"'{destinationType.FullName}': destination type '{destinationType.FullName}' " +
            "could not be created.",
            innerException);
    }

    private static MappingException CreateSourceAccessException(
        Type sourceType,
        Type destinationType,
        MappingContext context,
        Exception innerException)
    {
        var path = context.RenderPath();
        return new MappingException(
            sourceType,
            destinationType,
            path,
            $"Failed to read source member '{path}' from '{sourceType.FullName}' while mapping " +
            $"to '{destinationType.FullName}': the source getter threw an exception.",
            innerException);
    }

    private static MappingException CreateDestinationAccessException(
        Type sourceType,
        Type destinationType,
        MappingContext context,
        Exception innerException)
    {
        var path = context.RenderPath();
        return new MappingException(
            sourceType,
            destinationType,
            path,
            $"Failed to set destination member '{path}' from '{sourceType.FullName}' to " +
            $"'{destinationType.FullName}': the destination setter threw an exception.",
            innerException);
    }

    private static MappingException CreateCollectionEnumerationException(
        Type sourceType,
        Type destinationType,
        MappingContext context,
        int index,
        Exception innerException)
    {
        var path = context.RenderPath();
        return new MappingException(
            sourceType,
            destinationType,
            path,
            $"Failed to enumerate collection member '{path}' at index {index}: " +
            innerException.Message,
            innerException);
    }

    private static MappingException CreateCollectionWriteException(
        Type sourceType,
        Type destinationType,
        MappingContext context,
        int index,
        Exception innerException)
    {
        var path = context.RenderPath();
        return new MappingException(
            sourceType,
            destinationType,
            path,
            $"Failed to write collection element at '{path}' (index {index}): " +
            innerException.Message,
            innerException);
    }

    private static MappingException CreateMappingException(
        Type sourceType,
        Type destinationType,
        string memberPath,
        Type valueType,
        Type targetType,
        Exception? conversionException)
    {
        var reason = conversionException is null
            ? $"no conversion or nested map is available from '{valueType.FullName}' to '{targetType.FullName}'."
            : $"conversion from '{valueType.FullName}' to '{targetType.FullName}' failed: " +
              conversionException.Message;

        return new MappingException(
            sourceType,
            destinationType,
            memberPath,
            $"Failed to map member '{memberPath}' from '{sourceType.FullName}' to " +
            $"'{destinationType.FullName}': {reason}",
            conversionException);
    }

    private static MappingException CreateDictionaryMappingException(
        Type sourceType,
        Type destinationType,
        string memberPath,
        Type valueType,
        Type targetType)
    {
        return new MappingException(
            sourceType,
            destinationType,
            memberPath,
            $"Failed to map member '{memberPath}' from '{sourceType.FullName}' to " +
            $"'{destinationType.FullName}': dictionary mapping is not supported. " +
            $"Value type: '{valueType.FullName}', target type: '{targetType.FullName}'.");
    }

    private static MappingException CreateUnsupportedCollectionException(
        Type sourceType,
        Type destinationType,
        string memberPath,
        Type valueType,
        Type targetType,
        string reason)
    {
        return new MappingException(
            sourceType,
            destinationType,
            memberPath,
            $"Failed to map member '{memberPath}' from '{sourceType.FullName}' to " +
            $"'{destinationType.FullName}': {reason} " +
            $"Value type: '{valueType.FullName}', target type: '{targetType.FullName}'.");
    }

    private static string CreateCollectionTargetReason(Type valueType, Type targetType)
    {
        if (CollectionClassifier.GetDestinationInfo(targetType).IsSupported)
        {
            if (CollectionClassifier.IsCollection(valueType))
            {
                return $"collection source type '{valueType.FullName}' is not a supported generic " +
                       "collection. The source must implement a single IEnumerable<T>.";
            }

            return $"non-collection value of type '{valueType.FullName}' cannot be mapped to " +
                   $"collection type '{targetType.FullName}'.";
        }

        return $"collection destination type '{targetType.FullName}' is not supported. " +
               "Supported destinations are arrays, List<T>, IEnumerable<T>, ICollection<T>, " +
               "IList<T>, IReadOnlyCollection<T> and IReadOnlyList<T>.";
    }

    private static bool IsNullable(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
    }

    private static bool TryConvert(object value, Type destinationType, out object? converted)
    {
        var nonNullableDestination = Nullable.GetUnderlyingType(destinationType) ?? destinationType;
        var valueType = value.GetType();

        if (nonNullableDestination.IsInstanceOfType(value))
        {
            converted = value;
            return true;
        }

        if (nonNullableDestination.IsEnum)
        {
            if (value is string text)
            {
                converted = Enum.Parse(nonNullableDestination, text, ignoreCase: true);
                return true;
            }

            if (IsNumber(valueType))
            {
                converted = Enum.ToObject(nonNullableDestination, value);
                return true;
            }
        }

        if (nonNullableDestination == typeof(Guid) && value is string guidText)
        {
            converted = Guid.Parse(guidText);
            return true;
        }

        if (value is Guid guidValue && nonNullableDestination == typeof(string))
        {
            converted = guidValue.ToString();
            return true;
        }

        if (value is IConvertible)
        {
            converted = Convert.ChangeType(value, nonNullableDestination, CultureInfo.InvariantCulture);
            return true;
        }

        converted = null;
        return false;
    }

    private static bool IsNumber(Type type)
    {
        return type == typeof(byte) || type == typeof(sbyte) ||
               type == typeof(short) || type == typeof(ushort) ||
               type == typeof(int) || type == typeof(uint) ||
               type == typeof(long) || type == typeof(ulong) ||
               type == typeof(float) || type == typeof(double) ||
               type == typeof(decimal);
    }

    private sealed class MappingContext
    {
        private readonly Dictionary<MappingKey, object> _destinationsBySource =
            new(MappingKeyComparer.Instance);
        private readonly List<PathFrame> _path = new(8);
        private int _depth;

        public bool TryGet(object source, Type destinationType, out object destination)
        {
            return _destinationsBySource.TryGetValue(
                new MappingKey(source, destinationType),
                out destination!);
        }

        public void Register(object source, Type destinationType, object destination)
        {
            _destinationsBySource[new MappingKey(source, destinationType)] = destination;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterDepth(int maxDepth, Type sourceType, Type destinationType)
        {
            if (_depth >= maxDepth)
            {
                ThrowMaxDepthExceeded(maxDepth, sourceType, destinationType);
            }

            _depth++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitDepth()
        {
            _depth--;
        }

        [DoesNotReturn]
        private void ThrowMaxDepthExceeded(int maxDepth, Type sourceType, Type destinationType)
        {
            var path = RenderPath();
            throw new MappingException(
                sourceType,
                destinationType,
                path,
                $"Maximum mapping depth of {maxDepth} was exceeded at '{path}' while mapping " +
                $"from '{sourceType.FullName}' to '{destinationType.FullName}'. " +
                "Increase MapperConfigurationOptions.MaxDepth or simplify the object graph.");
        }

        public void PushMember(string memberName)
        {
            _path.Add(new PathFrame(memberName, null));
        }

        public void PushIndex(int index)
        {
            _path.Add(new PathFrame(null, index));
        }

        public void Pop()
        {
            _path.RemoveAt(_path.Count - 1);
        }

        public string RenderPath()
        {
            if (_path.Count == 0)
            {
                return "$";
            }

            var builder = new StringBuilder();

            foreach (var frame in _path)
            {
                if (frame.Index is { } index)
                {
                    builder.Append('[').Append(index).Append(']');
                }
                else if (frame.Member is not null)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append('.');
                    }

                    builder.Append(frame.Member);
                }
            }

            return builder.ToString();
        }
    }

    private readonly struct PathFrame
    {
        public PathFrame(string? member, int? index)
        {
            Member = member;
            Index = index;
        }

        public string? Member { get; }

        public int? Index { get; }
    }

    private readonly struct MappingKey
    {
        public MappingKey(object source, Type destinationType)
        {
            Source = source;
            DestinationType = destinationType;
        }

        public object Source { get; }

        public Type DestinationType { get; }
    }

    private sealed class MappingKeyComparer : IEqualityComparer<MappingKey>
    {
        public static readonly MappingKeyComparer Instance = new();

        public bool Equals(MappingKey x, MappingKey y)
        {
            return ReferenceEquals(x.Source, y.Source) &&
                   x.DestinationType == y.DestinationType;
        }

        public int GetHashCode(MappingKey key)
        {
            return HashCode.Combine(
                RuntimeHelpers.GetHashCode(key.Source),
                key.DestinationType);
        }
    }
}
