using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace SentinelProject.Consumer.Infrastructure;

public class StoredCustomerTransaction
{
    [BsonId]
    public ObjectId Id { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid TransactionId { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public required string Country { get; set; }
    public required string Merchant { get; set; }
    public required string Device { get; set; }
    public required string TransactionType { get; set; }
    public DateTime IssuesAt { get; set; }
}
