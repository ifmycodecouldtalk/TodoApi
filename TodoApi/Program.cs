using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);                                       // CREATES WebApplicationBuilder
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));      //      adds the database context to the 
builder.Services.AddDatabaseDeveloperPageExceptionFilter();                             //      dependency injection (DI) container
var app = builder.Build();

app.MapGet("/", () => "Hello World!");                                                  // Hello World upon "/" request

app.MapGet("/todoitems", async (TodoDb db) =>                                           //
    await db.Todos.ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDb db) =>                                  //
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>                              //
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound());

app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>                               //      maps post endpoint to add database
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>              //
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>                           //
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

app.Run();                                                                              // runs the app

class Todo                                                                              // 
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}

class TodoDb : DbContext                                                                // 
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}