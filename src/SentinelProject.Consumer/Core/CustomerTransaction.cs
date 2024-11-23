using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace SentinelProject.Consumer.Core;

public record CustomerTransaction(
    [property: BsonId]
    [property: BsonGuidRepresentation(GuidRepresentation.Standard)]
    Guid TransactionId,
    [property: BsonGuidRepresentation(GuidRepresentation.Standard)]
    Guid CustomerId,
    decimal Amount,
    string Country,
    string Merchant,
    string Device,
    string TransactionType,
    DateTime IssuesAt
    );
