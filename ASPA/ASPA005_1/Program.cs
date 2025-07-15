using Microsoft.AspNetCore.Diagnostics;
using DAL004;

var builder = WebApplication.CreateBuilder(args);

// Регистрируем Repository в контейнере зависимостей как Singleton
Repository.JSONFileName = "Celebrities.json";
builder.Services.AddSingleton<IRepository>(_ => Repository.Create("Celebrities"));

var app = builder.Build();

// Обработка ошибок
app.UseExceptionHandler("/Celebrities/Error");

// GET: получить всех знаменитостей
app.MapGet("/Celebrities", (IRepository repository) => repository.getAllCelebrities());

// GET: получить по id
app.MapGet("/Celebrities/{id:int}", (int id, IRepository repository) =>
{
    var celebrity = repository.getCelebrityById(id);
    if (celebrity == null)
        throw new FoundByIdException($"Celebrity Id = {id}");
    return celebrity;
});

// POST: добавить знаменитость с фильтрами
app.MapPost("/Celebrities", (Celebrity celebrity, IRepository repository) =>
{
    int? id = repository.addCelebrity(celebrity);
    if (id == null)
        throw new AddCelebrityException("/Celebrities error, id == null");
    if (repository.SaveChanges() <= 0)
        throw new SaveException("/Celebrities error, SaveChanges() <= 0");

    return new Celebrity((int)id, celebrity.Firstname, celebrity.Surname, celebrity.PhotoPath);
})
.AddEndpointFilter(async (context, next) =>
{
    var celebrity = context.GetArgument<Celebrity>(0);
    if (celebrity == null)
        throw new AddCelebrityException("/Celebrities error, Celebrity object is null");

    if (string.IsNullOrWhiteSpace(celebrity.Surname) || celebrity.Surname.Length < 2)
        throw new PostException("/Celebrities error, Surname is wrong");

    return await next(context);
})
.AddEndpointFilter(async (context, next) =>
{
    var celebrity = context.GetArgument<Celebrity>(0);
    if (celebrity == null)
        throw new AddCelebrityException("/Celebrities error, Celebrity object is null");

    var repository = context.HttpContext.RequestServices.GetRequiredService<IRepository>();

    if (repository.getAllCelebrities().Any(c => c.Surname == celebrity.Surname))
        throw new PostException("/Celebrities error, Surname is doubled");

    return await next(context);
})
.AddEndpointFilter(async (context, next) =>
{
    var celebrity = context.GetArgument<Celebrity>(0);
    if (celebrity == null)
        throw new AddCelebrityException("/Celebrities error, Celebrity object is null");

    var fileName = Path.GetFileName(celebrity.PhotoPath);
    var basePath = "C:\\Users\\user\\Desktop\\VS\\ТПвИ\\ASPA\\ASPA005_1\\bin\\Debug\\net8.0\\Celebrities";
    var fullPath = Path.Combine(basePath, fileName);

    if (!File.Exists(fullPath))
    {
        context.HttpContext.Response.Headers["X-Celebrity"] = $"Not Found = {fileName}";
    }

    return await next(context);
});

// PUT: обновить знаменитость
app.MapPut("/Celebrities/{id:int}", (int id, Celebrity celebrity, IRepository repository) =>
{
    int? updated = repository.updCelebrityById(id, celebrity);
    if (updated == -1)
        throw new PutException($"Can't update a celebrity with id {id}. This record does not exist");

    return $"Celebrity with id {updated} was updated";
});

// DELETE: удалить знаменитость
app.MapDelete("/Celebrities/{id:int}", (int id, IRepository repository) =>
{
    if (!repository.delCelebrityById(id))
        throw new FoundByIdException($"DELETE /Celebrities error, Id = {id}");

    return $"Celebrity with id {id} was deleted\n";
});

// Fallback
app.MapFallback((HttpContext ctx) =>
    Results.NotFound(new { error = $"path {ctx.Request.Path} not supported" }));

// Обработка ошибок
app.Map("/Celebrities/Error", (HttpContext ctx) =>
{
    Exception? ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
    IResult rc = Results.Problem(
        detail: "Panic",
        instance: app.Environment.EnvironmentName,
        title: "ASPA004",
        statusCode: 500);

    if (ex != null)
    {
        rc = ex switch
        {
            DeletionException => Results.NotFound(ex.Message),
            FoundByIdException => Results.NotFound(ex.Message),
            BadHttpRequestException => Results.BadRequest(ex.Message),
            SaveException => Results.Problem(title: "ASP004/SaveChanges", detail: ex.Message, instance: app.Environment.EnvironmentName, statusCode: 500),
            AddCelebrityException => Results.Problem(title: "ASP004/AddCelebrity", detail: ex.Message, instance: app.Environment.EnvironmentName, statusCode: 500),
            PostException => Results.Conflict(ex.Message),
            _ => rc
        };
    }

    return rc;
});

app.Run();


// Исключения
public class FoundByIdException : Exception
{
    public FoundByIdException(string message) : base($"Found by Id: {message}") { }
}

public class SaveException : Exception
{
    public SaveException(string message) : base($"SaveChanges error: {message}") { }
}

public class AddCelebrityException : Exception
{
    public AddCelebrityException(string message) : base($"AddCelebrityException error:{message}") { }
}

public class DeletionException : Exception
{
    public DeletionException(string message) : base(message) { }
}

public class PutException : Exception
{
    public PutException(string message) : base(message) { }
}

public class PostException : Exception
{
    public PostException(string message) : base($"Value:POST {message}") { }
}



