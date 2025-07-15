using DAL_Celebrity_MSSQL; // Теперь мы точно знаем, что нужно использовать этот IRepository и Repository
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text.Json;
using System.Collections.Generic; // Для List<T>
using System;
using static ANC25_WEBAPI_DLL.CelebritiesConfig;

namespace ANC25_WEBAPI_DLL
{
    public static class CelebrityAPI
    {
        // Метод расширения для подключения конфигурации IOptions<CelebritiesConfig>
        public static WebApplicationBuilder AddCelebritiesConfiguration(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<CelebritiesConfig>(builder.Configuration.GetSection("Celebrities"));
            return builder;
        }

        // Метод расширения для подключения сервисов: IRepository, CountryCodes, CelebrityTitles
        public static WebApplicationBuilder AddCelebritiesServices(this WebApplicationBuilder builder)
        {
            //используем DAL_Celebrity_MSSQL.Repository
            builder.Services.AddScoped<DAL_Celebrity_MSSQL.IRepository>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<CelebritiesConfig>>().Value;
                return new DAL_Celebrity_MSSQL.Repository(config.ConnectionString);
            });

            builder.Services.AddSingleton<CountryCodes>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<CelebritiesConfig>>().Value;
                string filePath = config.IS03166alpha2Path;
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Country codes file not found at {filePath}");
                }
                string json = File.ReadAllText(filePath);
                var countryCodesList = JsonSerializer.Deserialize<List<CountryCode>>(json);
                return new CountryCodes(countryCodesList);
            });

            builder.Services.AddSingleton<CelebrityTitles>();

            return builder;
        }

        // Метод расширения для маппинга эндпоинтов Celebrity (Minimal API)
        public static IEndpointRouteBuilder MapCelebrities(this IEndpointRouteBuilder app)
        {
            // Используем DAL_Celebrity_MSSQL.IRepository
            var group = app.MapGroup("/api/celebrities");

            group.MapGet("/", (DAL_Celebrity_MSSQL.IRepository repo) =>
            {
                // Использование синхронного метода GetAllCelebrities()
                return Results.Ok(repo.GetAllCelebrities());
            });

            group.MapGet("/{id:int}", (int id, DAL_Celebrity_MSSQL.IRepository repo) =>
            {
                // Использование синхронного метода GetCelebrityById()
                var celebrity = repo.GetCelebrityById(id);
                return celebrity != null ? Results.Ok(celebrity) : Results.NotFound();
            });

            group.MapPost("/", (Celebrity celebrity, DAL_Celebrity_MSSQL.IRepository repo) =>
            {
                // Использование синхронного метода AddCelebrity()
                if (repo.AddCelebrity(celebrity))
                {
                    // После добавления может потребоваться получить Id, если он генерируется БД
                    // Для простоты, пока возвращаем созданный объект
                    return Results.Created($"/api/celebrities/{celebrity.Id}", celebrity);
                }
                return Results.BadRequest("Could not add celebrity.");
            });

            group.MapPut("/{id:int}", (int id, Celebrity celebrity, DAL_Celebrity_MSSQL.IRepository repo) =>
            {
                if (id != celebrity.Id)
                {
                    return Results.BadRequest("ID mismatch.");
                }
                // Использование синхронного метода UpdateCelebrity()
                if (repo.UpdateCelebrity(id, celebrity))
                {
                    return Results.NoContent();
                }
                return Results.NotFound(); // Или другой статус, если не удалось обновить
            });

            group.MapDelete("/{id:int}", (int id, DAL_Celebrity_MSSQL.IRepository repo) =>
            {
                // Использование синхронного метода DeleteCelebrity()
                if (repo.DeleteCelebrity(id))
                {
                    return Results.NoContent();
                }
                return Results.NotFound(); // Или другой статус, если не удалось удалить
            });

            return app;
        }

        // Метод расширения для маппинга эндпоинтов Lifeevent (Minimal API)
        public static IEndpointRouteBuilder MapLifeevents(this IEndpointRouteBuilder app)
        {
            // Здесь будет логика для LifeEvent, аналогично Celebrities
            // Используем DAL_Celebrity_MSSQL.IRepository
            var group = app.MapGroup("/api/lifeevents");

            group.MapGet("/", (DAL_Celebrity_MSSQL.IRepository repo) =>
            {
                return Results.Ok(repo.GetAllLifeEvents());
            });

            group.MapGet("/{id:int}", (int id, DAL_Celebrity_MSSQL.IRepository repo) =>
            {
                var lifeEvent = repo.GetLifeEventById(id);
                return lifeEvent != null ? Results.Ok(lifeEvent) : Results.NotFound();
            });

            group.MapPost("/", (LifeEvent lifeEvent, DAL_Celebrity_MSSQL.IRepository repo) =>
            {
                if (repo.AddLifeEvent(lifeEvent))
                {
                    return Results.Created($"/api/lifeevents/{lifeEvent.Id}", lifeEvent);
                }
                return Results.BadRequest("Could not add life event.");
            });

            group.MapPut("/{id:int}", (int id, LifeEvent lifeEvent, DAL_Celebrity_MSSQL.IRepository repo) =>
            {
                if (id != lifeEvent.Id)
                {
                    return Results.BadRequest("ID mismatch.");
                }
                if (repo.UpdateLifeEvent(id, lifeEvent))
                {
                    return Results.NoContent();
                }
                return Results.NotFound();
            });

            group.MapDelete("/{id:int}", (int id, DAL_Celebrity_MSSQL.IRepository repo) =>
            {
                if (repo.DeleteLifeEvent(id))
                {
                    return Results.NoContent();
                }
                return Results.NotFound();
            });

            // Дополнительные методы для LifeEvent
            group.MapGet("/bycelebrity/{celebrityId:int}", (int celebrityId, DAL_Celebrity_MSSQL.IRepository repo) =>
            {
                return Results.Ok(repo.GetLifeEventsByCelebrityId(celebrityId));
            });

            group.MapGet("/celebrityfromlifeevent/{lifeEventId:int}", (int lifeEventId, DAL_Celebrity_MSSQL.IRepository repo) =>
            {
                var celebrity = repo.GetCelebrityByLifeEventId(lifeEventId);
                return celebrity != null ? Results.Ok(celebrity) : Results.NotFound();
            });

            return app;
        }

        // Метод расширения для маппинга эндпоинтов фотографий
        public static IEndpointRouteBuilder MapPhotoCelebrities(this IEndpointRouteBuilder app)
        {
            app.MapGet("/Photos/{fileName}", (string fileName, HttpContext context) =>
            {
                var config = context.RequestServices.GetRequiredService<IOptions<CelebritiesConfig>>().Value;
                var photoPath = Path.Combine(config.PhotosFolder, fileName);

                if (!File.Exists(photoPath))
                {
                    return Results.NotFound();
                }

                var contentType = "image/jpeg";
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                switch (extension)
                {
                    case ".png":
                        contentType = "image/png";
                        break;
                    case ".gif":
                        contentType = "image/gif";
                        break;
                        // Добавьте другие типы, если нужно
                }

                // Читаем файл синхронно, так как репозиторий тоже синхронный
                return Results.File(System.IO.File.ReadAllBytes(photoPath), contentType);
            });

            return app;
        }
    }
}