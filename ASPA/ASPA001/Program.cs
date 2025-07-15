using Microsoft.AspNetCore.HttpLogging;

internal class Program  
{
    private static void Main(string[] args) 
    {
        var builder = WebApplication.CreateBuilder(args);   // ������� ���-���������� � ��������� ������������

        // �������� ������ HTTP Logging � ����������� ����������� ���� ��� ������������
        builder.Services.AddHttpLogging(logging =>
        {
            // ���������, ����� ���� �������/������ ����� ������ � �����.
            logging.LoggingFields = HttpLoggingFields.RequestPath
                                    | HttpLoggingFields.RequestMethod
                                    | HttpLoggingFields.RequestHeaders
                                    | HttpLoggingFields.RequestBody
                                    | HttpLoggingFields.ResponseHeaders
                                    | HttpLoggingFields.ResponseBody;

            // ����� ��� ������������� ������ ������ �� ������������ ���� ������� � ������
            logging.RequestBodyLogLimit = 4096;   // 4 ��
            logging.ResponseBodyLogLimit = 4096;  // 4 ��
        });

        var app = builder.Build();  // ������� ���������

        //  ���������� middleware HTTP Logging.
        //    ��� ������ ���� �� ������ middleware, ������� ������������ ���� ������� (��������, UseRouting, MapControllers � ��.)
        app.UseHttpLogging();

        app.MapGet("/", () => "��� ������ ASPA");   // ������ �������� �����
        app.MapGet("/second", () => "ASPA");
        app.Run();  // ������ ���������� 
    }
}