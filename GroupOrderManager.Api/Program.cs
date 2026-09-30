using GroupOrderManager.Api;
using GroupOrderManager.Application.Claims;
using GroupOrderManager.Application.GroupOrderItems;
using GroupOrderManager.Application.GroupOrders;
using GroupOrderManager.Application.Participants;
using GroupOrderManager.Infrastructure.Persistence;
using GroupOrderManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using GroupOrderManager.Application.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GomDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("GomDatabase")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();

// Errors -> consistent ProblemDetails responses with the right status codes
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddScoped<IGroupOrderService, GroupOrderService>();
builder.Services.AddScoped<IGroupOrderItemService, GroupOrderItemService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// First in the pipeline, so it catches exceptions from everything after it.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/group-orders", async (CreateGroupOrderRequest request, IGroupOrderService service, ClaimsPrincipal user) =>
{
    var id = await service.CreateAsync(request, GetUserId(user));
    return Results.Created($"/group-orders/{id}", new { id });
})
.WithName("CreateGroupOrder")
.RequireAuthorization();

app.MapPost("/group-orders/{groupOrderId}/items", async (Guid groupOrderId, AddGroupOrderItemRequest request, IGroupOrderItemService service, ClaimsPrincipal user) =>
{
    if (groupOrderId != request.GroupOrderId)
        return Results.BadRequest("Route groupOrderId does not match request body.");

    var id = await service.AddAsync(request, GetUserId(user));
    return Results.Created($"/group-orders/{groupOrderId}/items/{id}", new { id });
})
.WithName("AddGroupOrderItem")
.RequireAuthorization();

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

app.MapPatch("/claims/{id}/paid", async (Guid id, IClaimService service, ClaimsPrincipal user) =>
{
    await service.MarkAsPaidAsync(id, GetUserId(user));
    return Results.NoContent();
})
.WithName("MarkClaimPaid")
.RequireAuthorization();

app.MapPatch("/claims/{id}/unpaid", async (Guid id, IClaimService service, ClaimsPrincipal user) =>
{
    await service.MarkAsUnpaidAsync(id, GetUserId(user));
    return Results.NoContent();
})
.WithName("MarkClaimUnpaid")
.RequireAuthorization();

app.MapPatch("/group-orders/{id}/close", async (Guid id, IGroupOrderService service, ClaimsPrincipal user) =>
{
    await service.CloseAsync(id, GetUserId(user));
    return Results.NoContent();
})
.WithName("CloseGroupOrder")
.RequireAuthorization();

app.MapGet("/group-orders/{id}/amount-owed", async (Guid id, IGroupOrderService service) =>
{
    var result = await service.GetAmountOwedAsync(id);
    return Results.Ok(result);
})
.WithName("GetAmountOwed");

app.MapPost("/auth/register", async (RegisterRequest request, IAuthService authService) =>
{
    var id = await authService.RegisterAsync(request);
    return Results.Created($"/users/{id}", new { id });
})
.WithName("Register");

app.MapPost("/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    var response = await authService.LoginAsync(request);
    return Results.Ok(response);
})
.WithName("Login");

app.Run();

// Reads the user id from the JWT "sub" claim. Only called from endpoints with RequireAuthorization(),
// so the user is always authenticated with a token we issued — and every token we issue contains "sub".
static Guid GetUserId(ClaimsPrincipal user) => Guid.Parse(user.FindFirst("sub")!.Value);
