using DotnetApp.Features.Auth;
using DotnetApp.GraphQL;
using DotnetApp.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Service registration. Each AddXxx groups one concern (see the extension methods),
// keeping this composition root readable as the app grows.
builder.Services.AddOpenApi();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddAuth();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddGraphQLApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// AuthN before AuthZ: who are you, then are you allowed.
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { status = "ok" }))
   .WithName("Root");

app.MapAuthEndpoints();   // REST: /auth/register, /auth/login

app.MapGraphQL();         // GraphQL: /graphql (data features + Nitro IDE)

app.Run();
