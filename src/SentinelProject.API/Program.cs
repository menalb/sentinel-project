using FastEndpoints;
using FastEndpoints.Swagger;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using NSwag;
using SentinelProject.API;
using SentinelProject.API.Features.ProcessTransaction;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("Transactions") ?? throw new ArgumentNullException();

var database = InitDatabase(connectionString);
var transactionsCollection = database.GetCollection<StoredProcessTransactionRequest>("process-transactions-requests");

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddSingleton(transactionsCollection)
    .AddFastEndpoints()
    .AddAuthorization()
    .AddAuthentication(ApiKeyAuth.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuth>(ApiKeyAuth.SchemeName, null);

builder.Services.SwaggerDocument(o =>
{
    o.EnableJWTBearerAuth = false;
    o.DocumentSettings = s =>
    {
        s.AddAuth(ApiKeyAuth.SchemeName, new()
        {
            Name = ApiKeyAuth.HeaderName,
            In = OpenApiSecurityApiKeyLocation.Header,
            Type = OpenApiSecuritySchemeType.ApiKey,
        });
    };
});

builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProcessedTransactionResultConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication()
    .UseAuthorization()
    .UseFastEndpoints()
    .UseSwaggerGen();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();


static IMongoDatabase InitDatabase(string connectionString)
{
    var client = new MongoClient(connectionString);
    var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention()
        };
    ConventionRegistry.Register(
        "Camel Case Convention",
        pack,
        t => true
        );
    return client.GetDatabase("sentinel-transactions-requests");
}