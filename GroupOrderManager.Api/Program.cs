using GroupOrderManager.Application.GroupOrders;
using GroupOrderManager.Infrastructure.Persistence;
using GroupOrderManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GomDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("GomDatabase")));

builder.Services.AddScoped<IGroupOrderService, GroupOrderService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapPost("/group-orders", async (CreateGroupOrderRequest request, IGroupOrderService service) =>
{
    var id = await service.CreateAsync(request);
    return Results.Created($"/group-orders/{id}", new { id });
})
.WithName("CreateGroupOrder");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
