var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddFastEndpoints();

builder.Services.AddSwaggerDoc(serializerSettings: _ =>
{
    _.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseOpenApi();

app.UseSwaggerUi3(_ => _.ConfigureDefaults());

app.UseAuthorization();

app.UseFastEndpoints(_ =>
    _.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

await app.RunAsync();
