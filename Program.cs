using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<JsonSerializerOptions>(o => o.PropertyNameCaseInsensitive = true);
builder.Services.AddControllers();

var app = builder.Build();
var UserDb = new List<User>();
var TagsCollection = new List<Tag>();

TagsCollection.Add(new Tag(1, "React js"));
TagsCollection.Add(new Tag(2, "Angular js"));
TagsCollection.Add(new Tag(3, "Laravel"));

User GetUserByToken(string Token, List<User> Users)
{
    var user = Users.FirstOrDefault(user => user.Token == Token);

    return user;
}

// Configure the HTTP request pipeline
app.MapGet("/", () => "Hello world");

#region Follow
// Profile
app.MapGet("profiles/{profileName}",async (HttpContext ctx, string profileName) =>
{
    var profile = UserDb.FirstOrDefault(user => user.UserName == profileName);
    
    await ctx.Response.WriteAsJsonAsync(new ProfileRequestEnv<User>(profile));
});

//Follow Profile
app.MapPost("/profiles/{profileName}/follow", async (HttpContext ctx, string profileName) =>
{
    var profile = UserDb.FirstOrDefault(user => user.UserName == profileName);

    var user = GetUserByToken(ctx.Request.Headers["Authorization"], UserDb); ;

    user.Follow.Add(profile);

    await ctx.Response.WriteAsJsonAsync(new ProfileRequestEnv<User>(profile));
});

app.MapDelete("/profiles/{PROFILENAME}/follow", (HttpContext ctx, string profileName) =>
{
    var profile = UserDb.FirstOrDefault(user => user.UserName == profileName);

    var user = GetUserByToken(ctx.Request.Headers["Authorization"], UserDb); ;

    user.Follow.Remove(profile);

    return ctx.Response.WriteAsJsonAsync(new ProfileRequestEnv<User>(profile));
});

#endregion

#region Tags

app.MapGet("/tags", async (ctx) =>
{ 
    await ctx.Response.WriteAsJsonAsync(new TagRequestEnv<Tag>(TagsCollection));
});

app.MapPost("/tags", async (HttpContext ctx, [FromBody] TagRequestEnv<TagRequest> req) =>
{
    Tag tag = new Tag(TagsCollection.Count + 1, req.Tags.First().name);
    TagsCollection.Add(tag);
    var list = new List<Tag>();
    list.Add(tag);
    await ctx.Response.WriteAsJsonAsync(new TagRequestEnv<Tag>(list)); 
});

app.MapPut("/tags", async (HttpContext ctx, [FromBody] TagRequestEnv<Tag> req) =>
{
    var tag = TagsCollection.FirstOrDefault(tag => tag.Id == req.Tags.First().Id);
    Tag newTag = new Tag(tag.Id, req.Tags.First().Name);
    TagsCollection.Remove(tag);
    TagsCollection.Add(newTag);
    var list = new List<Tag>();
    list.Add(newTag);
    await ctx.Response.WriteAsJsonAsync(new TagRequestEnv<Tag>(list));
});

app.MapDelete("/tags", (HttpContext ctx, [FromBody] TagRequestEnv<Tag> req) =>
{
    List<Tag> tags = new List<Tag>(req.Tags);

    foreach(var tag in req.Tags)
    {
        TagsCollection.Remove(tag);
    }

    return ctx.Response.WriteAsJsonAsync(new TagRequestEnv<Tag>(tags));
});


#endregion
app.MapControllers();
app.Run();


public class OldUser
{
    public string Username;
    public string Email;
    public string Password;
    public readonly string Token;
    public List<User> follow;

    public OldUser(string username, string email, string password)
    {
        Username = username;
        Email = email;
        Password = password;
    }

    public OldUser(User user)
    {
        this.Username = user.UserName;
        this.Password = user.Password;
        this.Email = user.Email;
        this.Token = user.Token;
        this.follow = new List<User>(user.Follow);
    }
}



public record UserRequest(string Username, string Email, string Password);

public record UserRequestEnv<T>(T User);

public record ProfileRequestEnv<T>(T Profile);

public record TagRequestEnv<T>(List<T> Tags);

public record TagRequest(string name);

public record Tag(int Id, string? Name);

public record User(string? UserName, string? Email, string? Password, string? Token,
    string? Bio, string? Image, List<User>? Follow);