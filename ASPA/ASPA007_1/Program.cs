using DAL_Celebrity_MSSQL;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

var celebritiesConfig = builder.Configuration.GetSection("Celebrities");
var connectionString = celebritiesConfig["ConnectionString"]
    ?? throw new Exception("ConnectionString ?? ????? ? ????????????");
var photosFolder = celebritiesConfig["PhotosFolder"]
    ?? throw new Exception("PhotosFolder ?? ????? ? ????????????");
var photosRequestPath = celebritiesConfig["PhotosRequestPath"]
    ?? throw new Exception("PhotosRequestPath ?? ????? ? ????????????");

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Celebrities", "/");
    options.Conventions.AddPageRoute("/NewCelebrity", "/0");
    options.Conventions.AddPageRoute("/Celebrity", "/Ñelebrity/{id:int:min(1)}");
});



builder.Services.AddSingleton<IRepository>(new Repository(connectionString));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!Directory.Exists(photosFolder))
{
    Directory.CreateDirectory(photosFolder);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(photosFolder)),
    RequestPath = photosRequestPath
});

app.UseRouting();

app.MapRazorPages();

app.Run();
