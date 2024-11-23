using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SentinelProject.Consumer.Infrastructure;

public class StoredCountry
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public float TrustRate { get; set; }
}
