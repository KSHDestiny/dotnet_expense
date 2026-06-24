using DotnetApp.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Service registration. Each AddXxx groups one concern (see the extension methods),
// keeping this composition root readable as the app grows.
builder.Services.AddOpenApi();
builder.Services.AddPersistence(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new { status = "ok" }))
   .WithName("Root");

app.Run();
