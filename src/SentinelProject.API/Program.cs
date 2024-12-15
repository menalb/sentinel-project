using FastEndpoints;
using FastEndpoints.Swagger;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using NSwag;
using SentinelProject.API.Consumers;
using SentinelProject.API.Features.ProcessTransaction;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.AddMongoDBClient("sentinel-transactions");

builder.Services
     .AddSingleton(ctx => {
         var client = ctx.GetRequiredService<IMongoClient>();
         var database = client.GetDatabase("sentinel-transactions-requests");
         return database.GetCollection<StoredProcessTransactionRequest>("process-transactions-requests");
         })
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
    x.AddConsumer<AcceptedTransactionResultConsumer>();
    x.AddConsumer<WarningTransactionResultConsumer>();
    x.AddConsumer<RejectedTransactionResultConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var configuration = context.GetRequiredService<IConfiguration>();
        var host = configuration.GetConnectionString("messaging");
        cfg.Host(host);
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention()
        };
ConventionRegistry.Register(
    "Camel Case Convention",
    pack,
    t => true
    );

app.MapDefaultEndpoints();

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