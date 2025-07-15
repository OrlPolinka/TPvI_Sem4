using static ANC25_WEBAPI_DLL.CelebritiesConfig;

namespace ANC25_WEBAPI_DLL
{
    public class CelebritiesConfig
    {
        public string PhotosFolder { get; set; }
        public string ConnectionString { get; set; }
        public string PhotosRequestPath { get; set; }
        public string IS03166alpha2Path { get; set; }
        public CelebritiesConfig() 
        {
            this.PhotosRequestPath = "/Photos";
            this.PhotosFolder = "./Photos";
            this.ConnectionString = "Server=WIN-FSAHKJTGC3M; Database=Lab6_Db; Trusted_Connection=true; TrustServerCertificate=Yes";
            this.IS03166alpha2Path = "C:\\Users\\user\\Desktop\\VS\\ТПвИ\\lab_8\\ASPA008_1\\CountryCodes\\Лабораторная_08__iso3166-1-alpha2-country-codes.json";
        }
        public class CountryCodes
        {
            public List<CountryCode> Codes { get; set; }
            public CountryCodes(List<CountryCode> codes)
            {
                Codes = codes;
            }
        }
        // Где-то в ANC25_WEBAPI_DLL (например, внутри CelebritiesConfig.cs или в отдельном файле)
        public class CountryCode
        {
            public string code { get; set; } // Соответствует "code" в JSON
            public string countryLabel { get; set; } // Соответствует "countryLabel" в JSON
        }

        public class CelebrityTitles
        {
            public List<string> Titles { get; set; } = new List<string> { "Младенец", "Ребенок", "Подросток", "Взрослый" };
        }
    }

}
