var builder = DistributedApplication.CreateBuilder(args);

var mongo = builder.AddMongoDB("mongo")
    .WithMongoExpress()
    .WithDataBindMount("/var/data/mongo")
    .AddDatabase("sentinel-transactions"); // adds default database, the parameter is the name to use to identify the resource
// mongo.AddDatabase("sentinel-transactions");
// mongo.AddDatabase("sentinel-transactions-requests");

var messaging = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin();

builder.AddProject<Projects.SentinelProject_API>("sentinelproject-api")
    .WithReference(mongo)
    .WaitFor(mongo)
    .WithReference(messaging);

builder.AddProject<Projects.SentinelProject_Consumer>("sentinelproject-consumer")
    .WithReference(mongo)
    .WaitFor(mongo)
    .WithReference(messaging);

builder.Build().Run();

