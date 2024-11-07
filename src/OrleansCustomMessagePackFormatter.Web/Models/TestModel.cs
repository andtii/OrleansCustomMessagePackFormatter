using MessagePack;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrleansCustomJsonConverter.Web.Models;

[MessagePackObject]
public class TestModel
{
    [Key(0)]
    public TestId Id { get; set; }
}

