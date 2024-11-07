using MessagePack;

namespace OrleansCustomJsonConverter.Web.Models;

[MessagePackObject]
public abstract record StronglyTypedId<TValue>(TValue Value)
    where TValue : notnull
{
    public override string ToString() => Value.ToString();
}
