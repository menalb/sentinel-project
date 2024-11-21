using FastEndpoints;
using FastEndpoints.Swagger;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services
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