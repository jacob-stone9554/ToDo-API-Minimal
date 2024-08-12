using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ToDoAPI_Minimal.Data;
using ToDoAPI_Minimal.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Items") ?? "Data Source=Items.db";

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSqlite<AppDbContext>(connectionString);
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

builder.Services.AddCors(options => { });

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
app.UseAuthorization();
app.MapControllers();

// Map all GET requests
app.MapGet("/", () => "Hello World!"); // Default route.
app.MapGet("/items", async (AppDbContext ctx) => await ctx.Items.ToListAsync()); // Return all items that are in the db
app.MapGet("/item/{id}", async (AppDbContext ctx, int id) => await ctx.Items.FindAsync(id)); //find the item in the db based on the id that was passed in.

// Map all POST requests


app.Run();
