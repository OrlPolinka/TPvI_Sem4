using Microsoft.AspNetCore.Diagnostics;
using DAL004;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


Repository.JSONFileName = "Celebrities.json";
using (IRepository repository = Repository.Create("Celebrities"))
{
    app.UseExceptionHandler("/Celebrities/Error");

    app.MapGet("/Celebrities", () => repository.getAllCelebrities()); // ASPA03

    app.MapGet("/Celebrities/{id:int}", (int id) =>
    {
        Celebrity? celebrity = repository.getCelebrityById(id);
        if (celebrity == null)
            throw new FoundByIdException($"Celebrity Id = {id}");
        return celebrity;
    });

    app.MapPost("/Celebrities", (Celebrity celebrity) =>
    {
        int? id = repository.addCelebrity(celebrity);
        if (id == null)
            throw new AddCelebrityException("/Celebrities error, id == null");
        if (repository.SaveChanges() <= 0)
            throw new SaveException("/Celebrities error, SaveChanges() <= 0");

        return new Celebrity((int)id, celebrity.Firstname, celebrity.Surname, celebrity.PhotoPath);
    });

   
    

    ///////////////////////////////////////////////////

    app.MapDelete("/Celebreties/{id:int}", (int id) =>
    {
        if (!repository.delCelebrityById(id)) throw new FoundByIdException($"DELETE /Celebrities error, Id = {id}");
        return $"Celebrity with id {id} was deleted\n";
    }
    );

    ///////////////////////////////////////////////////

    app.MapFallback((HttpContext ctx) => Results.NotFound(new { error = $"path {ctx.Request.Path} not supported" }));

    app.Map("/Celebrities/Error", (HttpContext ctx) =>
    {
        Exception? ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;    // получает исключение, вызвавшее ошибку.
        IResult rc = Results.Problem(detail: "Panic", instance: app.Environment.EnvironmentName, title: "ASPA004", statusCode: 500);    // возвращает структуру ошибки в формате ProblemDetails.

        if (ex != null)
        {
            if (ex is DeletionException)
            {
                rc = Results.NotFound(ex.Message);
            }
            if (ex is FoundByIdException)
            {
                rc = Results.NotFound(ex.Message);
            }
            if (ex is BadHttpRequestException)
            {
                rc = Results.BadRequest(ex.Message);
            }
            if (ex is SaveException)
            {
                rc = Results.Problem(title: "ASP004/SaveChanges", detail: ex.Message, instance: app.Environment.EnvironmentName, statusCode: 500);
            }
            if (ex is AddCelebrityException)
            {
                rc = Results.Problem(title: "ASP004/AddCelebrity", detail: ex.Message, instance: app.Environment.EnvironmentName, statusCode: 500);
            }
        }
        return rc;
    });

    //    if (ex != null)
    //    {
    //        if (ex is FoundByIdException)
    //            return Results.NotFound(new
    //            {
    //                title = "ASPA004/FoundById",
    //                status = 404,
    //                detail = ex.Message,
    //                instance = app.Environment.EnvironmentName
    //            });

    //        if (ex is BadHttpRequestException)
    //            return Results.BadRequest(new
    //            {
    //                title = "ASPA004/BadRequest",
    //                status = 400,
    //                detail = ex.Message,
    //                instance = app.Environment.EnvironmentName
    //            });

    //        if (ex is SaveException)
    //            return Results.Problem(
    //                title: "ASPA004/SaveChanges",
    //                detail: ex.Message,
    //                instance: app.Environment.EnvironmentName,
    //                statusCode: 500);

    //        if (ex is AddCelebrityException)
    //            return Results.Problem(
    //                title: "ASPA004/AddCelebrity",
    //                detail: ex.Message,
    //                instance: app.Environment.EnvironmentName,
    //                statusCode: 500);

    //        // Общая информация о системных исключениях
    //        return Results.Problem(
    //            title: "ASPA004/Unhandled",
    //            statusCode: 500,
    //            detail: $"Message: {ex.Message}\n" +
    //                    $"Exception Type: {ex.GetType().Name}\n" +
    //                    $"Source: {ex.Source}\n" +
    //                    $"StackTrace:\n{ex.StackTrace}",
    //            instance: app.Environment.EnvironmentName);
    //    }

    //    return Results.Problem(
    //        title: "ASPA004/Unknown",
    //        detail: "Unhandled error with no exception data.",
    //        instance: app.Environment.EnvironmentName,
    //        statusCode: 500);



    //    // Обрабатываются разные пользовательские исключения

    //    //    if (ex is FoundByIdException)   
    //    //        rc = Results.NotFound(ex.Message); // 404
    //    //    if (ex is BadHttpRequestException)
    //    //        rc = Results.BadRequest(ex.Message); // 400 ошибка в формате запроса
    //    //    if (ex is SaveException)
    //    //        rc = Results.Problem(title: "ASPA004/SaveChanges", detail: ex.Message, instance: app.Environment.EnvironmentName, statusCode: 500);
    //    //    if (ex is AddCelebrityException)
    //    //        rc = Results.Problem(title: "ASPA004/addCelebrity", detail: ex.Message, instance: app.Environment.EnvironmentName, statusCode: 500);
    //    //}
    //    //return rc;
    //});


    app.Run();
}

public class FoundByIdException : Exception     // когда не найден объект по ID.
{
    public FoundByIdException(string message) : base($"Found by Id: {message}") { }
}

public class SaveException : Exception  // если не удалось сохранить изменения
{
    public SaveException(string message) : base($"SaveChanges error: {message}") { }
}

public class AddCelebrityException : Exception      // если не удалось добавить нового объекта
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
