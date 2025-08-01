using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();



app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "Neumann.html" } // ��������� ��������� ��������
});

app.UseStaticFiles(); // ������ ����� �� wwwroot

// ������ ����� �� wwwroot ����� /static
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
