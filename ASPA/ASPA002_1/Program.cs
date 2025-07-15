var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ��������� ��������� "index.html" � �������� ����� �� ���������
app.UseDefaultFiles();

// �������� ��������� ����������� ������ (CSS, JS, HTML)
app.UseStaticFiles();

app.UseWelcomePage("/aspnetcore");

app.MapGet("/aspnetcore", () => "Hello World!");

app.Run();
