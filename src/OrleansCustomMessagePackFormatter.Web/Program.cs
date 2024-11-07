
using Orleans;
using Orleans.Runtime;
using Orleans.Hosting;
using Orleans.Configuration;
using OrleansCustomJsonConverter.Web;
using OrleansCustomJsonConverter.Web.Grains;
using OrleansCustomJsonConverter.Web.Models;
using System.Globalization;
using Orleans.Serialization;
using System.Text.Json;
using MessagePack;
using OrleansCustomMessagePackFormatter.Web.Models;
using MessagePack.Resolvers;

var builder = WebApplication.CreateBuilder(args);

var compositeResolver = CompositeResolver.Create(
    StronglyTypedIdResolver.Instance,
    StandardResolver.Instance
);

var options = MessagePackSerializerOptions.Standard
  .WithResolver(compositeResolver);
//.WithCompression(MessagePackCompression.Lz4BlockArray);


builder.Host.UseOrleans((ctx, siloBuilder) =>
 {
     siloBuilder.UseLocalhostClustering()
       .Configure((Action<ClusterOptions>)(options =>
       {
             options.ClusterId = $"cluster1";
             options.ServiceId = "cluster1";
         }));

     siloBuilder.Services.AddSerializer(serializerBuilder =>
     {
         serializerBuilder.AddMessagePackSerializer(
              null
              , null, options
             );
      
     });

 });


var app = builder.Build();
app.UseHttpsRedirection();

app.MapGet("/test", async (IClusterClient cluster1Client) =>
{
    var testModel = new TestModel
    {
        Id = new TestId(new Guid("d325f848-1192-459b-bc9b-8042a8113117"))
    };

    //This works
    //var serialized = JsonSerializer.Serialize(testModel);
    //var backToModel = JsonSerializer.Deserialize<TestModel>(serialized);

    //This works
    //var serializedBytes = MessagePackSerializer.Serialize(testModel, options);
    //backToModel = MessagePackSerializer.Deserialize<TestModel>(serializedBytes, options);

    //Passing model works
    var result =  await cluster1Client.GetGrain<ITestGrain>("").Dosomething(testModel);

    //Passing TestId directly works
    var result2 = await cluster1Client.GetGrain<ITestGrain>("").Dosomething2(testModel.Id);

    return result2;
});

await Task.WhenAll(app.RunAsync(), Task.Run(async () =>
{
    //Wait for the silo to start
    await Task.Delay(3000);
    var response = await new HttpClient().GetAsync("https://localhost:7069/test");
    var testModel = await response.Content.ReadFromJsonAsync<TestModel>();
    Console.WriteLine(testModel.Id);
    Console.ReadLine();

}));
