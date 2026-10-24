using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Mecesoft.ObjectMapper;

internal static class PropertyAccessor
{
    private static readonly ConcurrentDictionary<PropertyInfo, Func<object, object?>> Getters = new();
    private static readonly ConcurrentDictionary<PropertyInfo, Action<object, object?>> Setters = new();
    private static readonly ConcurrentDictionary<Type, Func<object>> Factories = new();

    public static Func<object, object?> CreateGetter(PropertyInfo property)
    {
        return Getters.GetOrAdd(property, static candidate => CreateGetterCore(candidate));
    }

    public static Action<object, object?> CreateSetter(PropertyInfo property)
    {
        return Setters.GetOrAdd(property, static candidate => CreateSetterCore(candidate));
    }

    public static Func<object> CreateFactory(Type type)
    {
        return Factories.GetOrAdd(type, static candidate => CreateFactoryCore(candidate));
    }

    private static Func<object, object?> CreateGetterCore(PropertyInfo property)
    {
        if (property.DeclaringType is null || property.DeclaringType.IsValueType)
        {
            return target => property.GetValue(target);
        }

        try
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var access = Expression.Property(
                Expression.Convert(instance, property.DeclaringType),
                property);
            var boxed = Expression.Convert(access, typeof(object));

            return Expression.Lambda<Func<object, object?>>(boxed, instance).Compile();
        }
        catch (Exception exception) when (!ExceptionHelper.IsFatal(exception))
        {
            return target => property.GetValue(target);
        }
    }

    private static Action<object, object?> CreateSetterCore(PropertyInfo property)
    {
        if (property.DeclaringType is null || property.DeclaringType.IsValueType)
        {
            return (target, value) => property.SetValue(target, value);
        }

        try
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");
            var assignment = Expression.Assign(
                Expression.Property(
                    Expression.Convert(instance, property.DeclaringType),
                    property),
                Expression.Convert(value, property.PropertyType));

            return Expression.Lambda<Action<object, object?>>(assignment, instance, value).Compile();
        }
        catch (Exception exception) when (!ExceptionHelper.IsFatal(exception))
        {
            return (target, value) => property.SetValue(target, value);
        }
    }

    private static Func<object> CreateFactoryCore(Type type)
    {
        if (type.IsValueType || type.GetConstructor(Type.EmptyTypes) is not null)
        {
            try
            {
                Expression body = Expression.New(type);
                if (type.IsValueType)
                {
                    body = Expression.Convert(body, typeof(object));
                }

                return Expression.Lambda<Func<object>>(body).Compile();
            }
            catch (Exception exception) when (!ExceptionHelper.IsFatal(exception))
            {
                // Fall back to reflection below.
            }
        }

        return () => Activator.CreateInstance(type)!;
    }
}
