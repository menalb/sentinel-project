using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SentinelProject.Consumer.Infrastructure;

public class StoredCustomer
{

    [BsonId]
    public ObjectId Id { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Unspecified)]
    public Guid CustomerId { get; set; }
    public required string Name { get; set; }
    public decimal MaxTransactionAmount { get; set; }

}
