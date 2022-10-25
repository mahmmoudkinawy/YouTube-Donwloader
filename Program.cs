var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddFastEndpoints();

builder.Services.AddCors(_ =>
{
    _.AddPolicy("Client", _ =>
    {
        _
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSwaggerDoc(serializerSettings: _ =>
{
    _.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseOpenApi();

app.UseSwaggerUi3(_ => _.ConfigureDefaults());

app.UseCors("Client");

app.UseAuthorization();

app.UseFastEndpoints(_ =>
    _.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

await app.RunAsync();
