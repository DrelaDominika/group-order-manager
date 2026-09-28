using GroupOrderManager.Application.Claims;
using GroupOrderManager.Application.GroupOrderItems;
using GroupOrderManager.Application.GroupOrders;
using GroupOrderManager.Application.Participants;
using GroupOrderManager.Infrastructure.Persistence;
using GroupOrderManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GomDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("GomDatabase")));

builder.Services.AddScoped<IGroupOrderService, GroupOrderService>();
builder.Services.AddScoped<IGroupOrderItemService, GroupOrderItemService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();
builder.Services.AddScoped<IClaimService, ClaimService>();

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

app.MapPost("/group-orders/{groupOrderId}/items", async (Guid groupOrderId, AddGroupOrderItemRequest request, IGroupOrderItemService service) =>
{
    if (groupOrderId != request.GroupOrderId)
        return Results.BadRequest("Route groupOrderId does not match request body.");

    var id = await service.AddAsync(request);
    return Results.Created($"/group-orders/{groupOrderId}/items/{id}", new { id });
})
.WithName("AddGroupOrderItem");

app.MapPost("/group-orders/{groupOrderId}/participants", async (Guid groupOrderId, AddParticipantRequest request, IParticipantService service) =>
{
    if (groupOrderId != request.GroupOrderId)
        return Results.BadRequest("Route groupOrderId does not match request body.");

    var id = await service.AddAsync(request);
    return Results.Created($"/group-orders/{groupOrderId}/participants/{id}", new { id });
})
.WithName("AddParticipant");

app.MapPost("/items/{itemId}/claims", async (Guid itemId, ClaimItemRequest request, IClaimService service) =>
{
    if (itemId != request.GroupOrderItemId)
        return Results.BadRequest("Route itemId does not match request body.");

    var id = await service.ClaimAsync(request);
    return Results.Created($"/items/{itemId}/claims/{id}", new { id });
})
.WithName("ClaimItem");

app.MapGet("/group-orders/{id}", async (Guid id, IGroupOrderService service) =>
{
    var order = await service.GetByIdAsync(id);
    return order is null ? Results.NotFound() : Results.Ok(order);
})
.WithName("GetGroupOrder");

app.MapPatch("/claims/{id}/paid", async (Guid id, IClaimService service) =>
{
    await service.MarkAsPaidAsync(id);
    return Results.NoContent();
})
.WithName("MarkClaimPaid");

app.MapPatch("/claims/{id}/unpaid", async (Guid id, IClaimService service) =>
{
    await service.MarkAsUnpaidAsync(id);
    return Results.NoContent();
})
.WithName("MarkClaimUnpaid");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
