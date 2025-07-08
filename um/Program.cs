
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpLogging((o) => { });

var app = builder.Build();

// In-memory list of users
var users = new List<User>
{
    new User { UserId = Guid.NewGuid(), UserName = "Alice", UserAge = 30 },
    new User { UserId= Guid.NewGuid(), UserName = "Bob", UserAge = 25 }
};

app.UseHttpLogging();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            error = "Something went wrong.",
            timestamp = DateTime.UtcNow
        };

        var json = System.Text.Json.JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(json);
    });
});

// Create user
app.MapPost("/users", (User newUser) =>
{
    if (string.IsNullOrWhiteSpace(newUser.UserName))
        return Results.BadRequest("UserName is required.");
    newUser.UserId = Guid.NewGuid(); // Generate a new UserId
    users.Add(newUser);
    return Results.Created($"/users/{newUser.UserName}", newUser);
});

// Read all users
app.MapGet("/users", () => users);

// Read one user by ID
app.MapGet("/users/{id:guid}", (Guid id) =>
{
    var user = users.FirstOrDefault(u => u.UserId == id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
});

// Update user by ID
app.MapPut("/users/{id:guid}", (Guid id, User updatedUser) =>
{
    var index = users.FindIndex(u => u.UserId == id);
    if (index == -1)
        return Results.NotFound();

    updatedUser.UserId = id; // Ensure the UserId remains the same
    users[index] = updatedUser;
    return Results.Ok(updatedUser);
});

// Delete user by ID
app.MapDelete("/users/{id:guid}", (Guid id) =>
{
    var user = users.FirstOrDefault(u => u.UserId == id);
    if (user is null)
        return Results.NotFound();

    users.Remove(user);
    return Results.Ok(user);
});


app.Run();


public class User
{
    public Guid UserId { get; set; } // Assuming UserId is auto-generated
    public required string UserName { get; set; }
    public int UserAge { get; set; }
}