using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;
using Microsoft.OpenApi.Models;
using ToDoAPI_Minimal.Data;
using ToDoAPI_Minimal.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Items") ?? "Data Source=Items.db";

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSqlite<AppDbContext>(connectionString); //add the SQLite database provider to the container, making it accessible.
builder.Services.AddSwaggerGen(c =>
{
    // Add basic documentation and info about the api to swagger
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "To-Do API",
        Description = "A simple to-do list API",
        Version = "v1"
    });
});

/*
 * Add CORS so that the API can send/receive data from the front-end,
 *      only from allowed, specified hosts.
 *      
 * Please note, any origin is currently allowed to request resources.
 * In a case where resources should be non-public, restrict to only allowed origins.
 */
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AnyOrigin",
        cfg =>
        {
            cfg.AllowAnyOrigin();
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
        });
}); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "To-Do API V1");
    });
}


app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AnyOrigin");
app.UseAuthorization();
app.MapControllers();


/*
 * Map all GET requests
 *  - / => returns "Hello World"
 *  - /items => returns a list of all items currently in the database.
 *  - /item/{id} => returns a specific item with a matching id.
 */
app.MapGet("/", () => "Hello World!"); // Default route.
app.MapGet("/items", async (AppDbContext ctx) => await ctx.Items.ToListAsync()); // Return all items that are in the database.
app.MapGet("/item/{id}", async (AppDbContext ctx, int id) => await ctx.Items.FindAsync(id)); //find the item in the database based on the id that was passed in.

/*
 * Map all POST requests
 *  - /item => adds a new item to the database.
 */
app.MapPost("/item", async (AppDbContext ctx, Item item) =>
{
    await ctx.Items.AddAsync(item); 
    await ctx.SaveChangesAsync(); 

    return Results.Created($"/item/{item.Id}", item); // Send a response with a 201 status code with the given Item object attached.
});

/*
 * Map all PUT requests
 *  - /item/{id} => updates a specific item in the database based on the id that was passed in
 *      alongside the updateItem (which contains the new values)
 */
app.MapPut("/item/{id}", async (AppDbContext ctx, Item updateItem, int id) =>
{
    var item = await ctx.Items.FindAsync(id); 

    if (item is null) return Results.NotFound(); // If the item is null, that means it does not exist. Return with a 404 Not Found status code.

    //update the item fields.
    item.Name = updateItem.Name; 
    item.Description = updateItem.Description;
    item.Type = updateItem.Type;
    item.isCompleted = updateItem.isCompleted;

    await ctx.SaveChangesAsync(); 

    return Results.NoContent(); // Send a response with a 204 status code.
});

/*
 * Map all DELETE requests
 *  - /item/{id} => deletes a specific item in the database, based on the id that was passed in.
 */
app.MapDelete("item/{id}", async (AppDbContext ctx, int id) =>
{
    var item = await ctx.Items.FindAsync(id);

    if (item is null) return Results.NotFound(); // If the item is null, that means it does not exist. Return with a 404 Not Found status code.
    ctx.Items.Remove(item); 
    await ctx.SaveChangesAsync();

    return Results.Ok(); // Send a response with a 201 status code.
});

app.Run();
