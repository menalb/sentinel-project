using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SentinelProject.API.Features.ProcessTransaction;

public class StoredProcessTransactionRequest
{
    [BsonId]
    public ObjectId Id { get; set; }
    public required ProcessTransactionRequest ProcessRequest { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public ProcessResult? Result { get; set; }
    
}

public class ProcessResult
{
    public required string Outcome { get; set; }
    public string? Message { get; set; }
}