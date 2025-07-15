using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();



app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "Neumann.html" } // Указываем стартовую страницу
});

app.UseStaticFiles(); // Раздаём файлы из wwwroot

// Раздаём файлы из wwwroot через /static
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/static" 
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Picture")),
    RequestPath = "/picture"
});

app.Run();
