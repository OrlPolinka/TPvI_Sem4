var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Позволяет загружать "index.html" и подобные файлы по умолчанию
app.UseDefaultFiles();

// Включает поддержку статических файлов (CSS, JS, HTML)
app.UseStaticFiles();

app.UseWelcomePage("/aspnetcore");

app.MapGet("/aspnetcore", () => "Hello World!");

app.Run();
