using DotnetApp.Common;
using DotnetApp.Features.Auth;
using DotnetApp.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Service registration. Each AddXxx groups one concern (see the extension methods),
// keeping this composition root readable as the app grows.
builder.Services.AddOpenApi();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddAuth();
builder.Services.AddJwtAuth(builder.Configuration);

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

app.MapAuthEndpoints();

// Reads identity via the injected ICurrentUser accessor (no inline claim parsing).
app.MapGet("/me", (ICurrentUser currentUser) => Results.Ok(new
{
    id = currentUser.Id,
    email = currentUser.Email
}))
.RequireAuthorization();

app.Run();
