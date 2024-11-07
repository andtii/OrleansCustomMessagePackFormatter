using MessagePack.Formatters;
using MessagePack;
using OrleansCustomJsonConverter.Web.Models;

namespace OrleansCustomMessagePackFormatter.Web.Models
{
    public class StronglyTypedIdResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new StronglyTypedIdResolver();

        private StronglyTypedIdResolver() { }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            var type = typeof(T);
            if (!StronglyTypedIdHelper.IsStronglyTypedId(type, out var idType))
            {
                return null;
            }

            var genericFormatterType = typeof(StronglyTypedIdMessagePackFormatter<,>).MakeGenericType(type, idType);
            var formatter = (IMessagePackFormatter<T>)Activator.CreateInstance(genericFormatterType);
            return formatter;
        }
    }
}
