using Microsoft.AspNetCore.HttpLogging;

internal class Program  
{
    private static void Main(string[] args) 
    {
        var builder = WebApplication.CreateBuilder(args);   // создаем веб-приложение и загружаем конфигурацию

        // Включаем сервис HTTP Logging и настраиваем необходимые поля для логгирования
        builder.Services.AddHttpLogging(logging =>
        {
            // Указываем, какие поля запроса/ответа хотим видеть в логах.
            logging.LoggingFields = HttpLoggingFields.RequestPath
                                    | HttpLoggingFields.RequestMethod
                                    | HttpLoggingFields.RequestHeaders
                                    | HttpLoggingFields.RequestBody
                                    | HttpLoggingFields.ResponseHeaders
                                    | HttpLoggingFields.ResponseBody;

            // Можно при необходимости задать лимиты на логгирование тела запроса и ответа
            logging.RequestBodyLogLimit = 4096;   // 4 КБ
            logging.ResponseBodyLogLimit = 4096;  // 4 КБ
        });

        var app = builder.Build();  // создаем экземпляр

        //  Подключаем middleware HTTP Logging.
        //    Оно должно идти до других middleware, которые обрабатывают тело запроса (например, UseRouting, MapControllers и пр.)
        app.UseHttpLogging();

        app.MapGet("/", () => "Мое первое ASPA");   // задаем конечную точку
        app.MapGet("/second", () => "ASPA");
        app.Run();  // запуск приложения 
    }
}