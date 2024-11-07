using MessagePack;
using System.Text.Json.Serialization;

namespace OrleansCustomJsonConverter.Web.Models;

[JsonConverter(typeof(StronglyTypedIdJsonConverter<TestId, Guid>))]

public record TestId(Guid Id) : StronglyTypedId<Guid>(Id);