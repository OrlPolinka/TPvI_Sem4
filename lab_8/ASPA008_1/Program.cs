using DAL_Celebrity_MSSQL;
using ANC25_WEBAPI_DLL;

var builder = WebApplication.CreateBuilder(args);

// ANC25_WEBAPI_DLL
builder.AddCelebritiesConfiguration();
builder.AddCelebritiesServices();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ASPA008_1.Filters.CelebrityWikipediaLinksAsyncFilter>();
var app = builder.Build();


app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapCelebrities();
app.MapLifeevents();
app.MapPhotoCelebrities();


app.MapControllerRoute(
    name: "new_celebrity_form",
    pattern: "/0",
    defaults: new { Controller = "Celebrities", Action = "NewHumanForm" });

app.MapControllerRoute(
    name: "celebrity_details",
    pattern: "/{id:int:min(1)}",
    defaults: new { Controller = "Celebrities", Action = "Human" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Celebrities}/{action=Index}/{id?}");

app.Run();