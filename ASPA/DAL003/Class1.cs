using System.Text.Json;
using System.IO;        
using System.Linq;      


namespace DAL003
{

    public interface IRepository:IDisposable
    {
        string BasePath { get; }                                // полный директорий для JSON и фотографий
        Celebrity[] getAllCelebrities();                        // получить весь список знаменитостей
        Celebrity? getCelebrityById(int id);                    // получить знаменитость по id
        Celebrity[] getCelebritiesBySurname(string Surname);    // получить знаменитость по фамилии
        string? getPhotoPathById(int id);                       // получить путь для GET-запроса к фотографии
    }

    public record Celebrity (int Id, string Firstname, string Surname, string PhotoPath);

    public class Repository : IRepository
    {
        private bool disposed = false;   
        private Celebrity[] celebrities; 

        public static string JSONFileName;

        public string BasePath { get; }

        public Repository(string relativePath)
        {
            BasePath = Path.Combine(AppContext.BaseDirectory, relativePath);


            string jsonPath = Path.Combine(BasePath, JSONFileName);

            if (!File.Exists(jsonPath))
                throw new FileNotFoundException($"Файл {JSONFileName} не найден по пути: {jsonPath}");

            string json = File.ReadAllText(jsonPath);

            celebrities = JsonSerializer.Deserialize<Celebrity[]>(json) ?? [];
        }

        public static IRepository Create(string relativePath)
        {
            return new Repository(relativePath);
        }


        public Celebrity[] getAllCelebrities()
        {
            return celebrities;
        }

        public Celebrity? getCelebrityById(int id)
        {
            return celebrities.FirstOrDefault(c => c.Id == id);
        }

        public Celebrity[] getCelebritiesBySurname(string surname)
        {
            return celebrities
                .Where(c => string.Equals(c.Surname, surname, StringComparison.OrdinalIgnoreCase)) // Без учёта регистра
                .ToArray(); 
        }

        public string? getPhotoPathById(int id)
        {
            var celeb = getCelebrityById(id);

            if (celeb == null || string.IsNullOrWhiteSpace(celeb.PhotoPath))
                return string.Empty;

            return celeb.PhotoPath.Trim();  // Trim - удаляет лишние пробелы

        }

        

        // Метод освобождения ресурсов
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
            }
        }
    }

}
