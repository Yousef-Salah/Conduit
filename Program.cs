using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<JsonSerializerOptions>(o => o.PropertyNameCaseInsensitive = true);

var app = builder.Build();
var UserDb = new List<Users>();

// Configure the HTTP request pipeline
app.MapGet("/", () => "Hello world");

// Register
app.MapPost("/users", async (HttpContext ctx, [FromBody] UserRequestEnv<UserRequest> req) =>
{
    var resp = new Users(req.User.Username, req.User.Email, req.User.Password, $"{Guid.NewGuid()}", "", "");
    UserDb.Add(resp);
    await ctx.Response.WriteAsJsonAsync(new UserRequestEnv<Users>(resp));
});

//Get Current User
app.MapGet("/user", (HttpRequest req) =>
{
    var user = UserDb.FirstOrDefault(x => x.Token == req.Headers["Authorization"]);
    return new UserRequestEnv<Users>(user);
});

// Update User Data
app.MapPut("/user", async (ctx) =>
{
    var body = "";

    using (var reader = new StreamReader(ctx.Request.Body))
    {
        body = await reader.ReadToEndAsync();
    }

    var req = JsonSerializer.Deserialize<UserRequestEnv<UserRequest>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    var user = UserDb.FirstOrDefault(x => x.Token == ctx.Request.Headers["Authorization"]);
    
    OldUser OldUser = new OldUser(user);

    if (req.User.Username != "")
    {
        OldUser.Username = req.User.Username;
    }

    if (req.User.Email != "")
    {
        OldUser.Email = req.User.Email;
    }

    if (req.User.Password != "")
    {
        OldUser.Password = req.User.Password;
    }

    if (req.User.Email != "")
    {
        OldUser.Email = req.User.Email;
    }

    UserDb.Remove(user);
    var resp = new Users(OldUser.Username, OldUser.Email, OldUser.Password, $"{ctx.Request.Headers["Authorization"]}", "", "");
    UserDb.Add(resp);
    await ctx.Response.WriteAsJsonAsync(new UserRequestEnv<Users>(resp));
});

app.MapPost("/users/login", (HttpContext ctx, [FromBody] UserRequestEnv<UserRequest> req) =>
{
    if(req.User.Username != "" && req.User.Password != "")
    {
        var user = UserDb.FirstOrDefault(x => x.Email == req.User.Email);
        if(user.Password == req.User.Password)
        {
            //user.Token = Guid.NewGuid();
            var userWithNewToken = new Users(user.UserName, user.Email, user.Password, Guid.NewGuid().ToString(), "", "");
            UserDb.Remove(user);
            UserDb.Add(userWithNewToken);

            return new UserRequestEnv<Users>(userWithNewToken);
        }
    }
    var EmptyUser = new Users("", "", "", "", "", "");
    return new UserRequestEnv<Users>(EmptyUser);
});


app.Run();


public class OldUser
{
    public string Username;
    public string Email;
    public string Password;
    public readonly string Token;

    public OldUser(string username, string email, string password)
    {
        Username = username;
        Email = email;
        Password = password;
    }

    public OldUser(Users user)
    {
        this.Username = user.UserName;
        this.Password = user.Password;
        this.Email = user.Email;
        this.Token = user.Token;
    }
}



public record UserRequest(string Username, string Email, string Password);

public record UserRequestEnv<T>(T User);

public record Users(string? UserName, string? Email, string? Password, string? Token,
    string? Bio, string? Image);