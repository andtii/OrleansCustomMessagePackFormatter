using MessagePack;
using MessagePack.Formatters;
using OrleansCustomJsonConverter.Web.Models;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace OrleansCustomMessagePackFormatter.Web.Models
{
    public class StronglyTypedIdMessagePackFormatter<TStronglyTypedId, TValue> : IMessagePackFormatter<TStronglyTypedId>
    where TStronglyTypedId : StronglyTypedId<TValue>
    where TValue : notnull
    {
        private readonly IMessagePackFormatter<TValue> _valueFormatter;
        private readonly Func<TValue, TStronglyTypedId> _factory;

        public StronglyTypedIdMessagePackFormatter()
        {
            // Get the formatter for TValue
            _valueFormatter = MessagePackSerializerOptions.Standard.Resolver.GetFormatterWithVerify<TValue>();

            // Get the factory method to create TStronglyTypedId instances
            _factory = StronglyTypedIdMessagePackHelper.GetFactory<TValue, TStronglyTypedId>();
        }

        public void Serialize(ref MessagePackWriter writer, TStronglyTypedId value, MessagePackSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNil();
                return;
            }

            _valueFormatter.Serialize(ref writer, value.Value, options);
        }

        public TStronglyTypedId Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return default;
            }

            var value = _valueFormatter.Deserialize(ref reader, options);
            return _factory(value);
        }
    }

    internal static class StronglyTypedIdMessagePackHelper
    {
        // Key: (TValue type, TStronglyTypedId type)
        private static readonly ConcurrentDictionary<(Type, Type), Delegate> StronglyTypedIdFactories = new();

        public static Func<TValue, TStronglyTypedId> GetFactory<TValue, TStronglyTypedId>()
            where TValue : notnull
            where TStronglyTypedId : StronglyTypedId<TValue>
        {
            var key = (typeof(TValue), typeof(TStronglyTypedId));
            if (!StronglyTypedIdFactories.TryGetValue(key, out var factory))
            {
                factory = CreateFactory<TValue, TStronglyTypedId>();
                StronglyTypedIdFactories.TryAdd(key, factory);
            }

            return (Func<TValue, TStronglyTypedId>)factory;
        }

        private static Func<TValue, TStronglyTypedId> CreateFactory<TValue, TStronglyTypedId>()
            where TValue : notnull
            where TStronglyTypedId : StronglyTypedId<TValue>
        {
            var stronglyTypedIdType = typeof(TStronglyTypedId);
            if (!IsStronglyTypedId(stronglyTypedIdType, out var idType) || idType != typeof(TValue))
            {
                throw new ArgumentException($"Type '{stronglyTypedIdType}' is not a strongly-typed ID type with TValue '{typeof(TValue)}'.", nameof(stronglyTypedIdType));
            }

            var ctor = stronglyTypedIdType.GetConstructor(new[] { typeof(TValue) });
            if (ctor == null)
            {
                throw new ArgumentException($"Type '{stronglyTypedIdType}' does not have a constructor with a single parameter of type '{typeof(TValue)}'.", nameof(stronglyTypedIdType));
            }

            var param = Expression.Parameter(typeof(TValue), "value");
            var body = Expression.New(ctor, param);
            var lambda = Expression.Lambda<Func<TValue, TStronglyTypedId>>(body, param);
            return lambda.Compile();
        }

        public static bool IsStronglyTypedId(Type type, out Type idType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.BaseType is Type baseType &&
                baseType.IsGenericType &&
                baseType.GetGenericTypeDefinition() == typeof(StronglyTypedId<>))
            {
                idType = baseType.GetGenericArguments()[0];
                return true;
            }

            idType = null;
            return false;
        }
    }
}
