using System.Text.Json;
using System.IO;
using System.Linq;
using DAL004;

namespace DAL004
{
    public interface IRepository : IDisposable
    {
        string BasePath { get; }                                // полный директорий для JSON и фотографий
        Celebrity[] getAllCelebrities();                        // получить весь список знаменитостей
        Celebrity? getCelebrityById(int id);                    // получить знаменитость по id
        Celebrity[] getCelebritiesBySurname(string Surname);    // получить знаменитость по фамилии
        string? getPhotoPathById(int id);                       // получить путь для GET-запроса к фотографии
        int? addCelebrity(Celebrity celebrity);                 // добавить знаменитость, = Id новой знаменитости
        bool delCelebrityById(int id);                          // удалить знаменитость по Id, = true - успех
        int? updCelebrityById(int id, Celebrity celebrity);     // изменить знаменитость по Id, = Id - новый Id - успех
        int SaveChanges();                                      // сохранить изменения в JSON, = количество изменений
    }

    public record Celebrity(int Id, string Firstname, string Surname, string PhotoPath);

    public class Repository : IRepository
    {
        private bool disposed = false;
        private List<Celebrity> celebrities;
        private bool isModified = false;

        public static string JSONFileName;

        public string BasePath { get; }

        public Repository(string relativePath)
        {
            BasePath = Path.Combine(AppContext.BaseDirectory, relativePath);

            string jsonPath = Path.Combine(BasePath, JSONFileName);

            if (!File.Exists(jsonPath))
                throw new FileNotFoundException($"Файл {JSONFileName} не найден по пути: {jsonPath}");

            string json = File.ReadAllText(jsonPath);

            // Читаем в список для удобной модификации
            celebrities = JsonSerializer.Deserialize<List<Celebrity>>(json) ?? new List<Celebrity>();
        }

        public static IRepository Create(string relativePath)
        {
            return new Repository(relativePath);
        }

        public Celebrity[] getAllCelebrities()
        {
            return celebrities.ToArray();
        }

        public Celebrity? getCelebrityById(int id)
        {
            return celebrities.FirstOrDefault(c => c.Id == id);
        }

        public Celebrity[] getCelebritiesBySurname(string surname)
        {
            return celebrities
                .Where(c => string.Equals(c.Surname, surname, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        public string? getPhotoPathById(int id)
        {
            var celeb = getCelebrityById(id);
            if (celeb == null || string.IsNullOrWhiteSpace(celeb.PhotoPath))
                return string.Empty;

            return celeb.PhotoPath.Trim();
        }

        public int? addCelebrity(Celebrity celebrity)
        {
            if (celebrity == null)
                return null;

            int newId = celebrities.Any() ? celebrities.Max(c => c.Id) + 1 : 1;

            var newCelebrity = new Celebrity(
                newId,
                celebrity.Firstname,
                celebrity.Surname,
                celebrity.PhotoPath
            );

            celebrities.Add(newCelebrity);
            isModified = true;
            return newCelebrity.Id;

        }

        public bool delCelebrityById(int id)
        {
            var celeb = getCelebrityById(id);
            if (celeb != null)
            {
                celebrities.Remove(celeb);
                isModified = true;
                return true;
            }
            return false;
        }

        public int? updCelebrityById(int id, Celebrity updatedCelebrity)
        {
            var index = celebrities.FindIndex(c => c.Id == id);
            if (index >= 0)
            {
                var newCelebrity = new Celebrity(
                    id/* = celebrities.Any() ? celebrities.Max(c => c.Id) + 1 : 1*/,
                    updatedCelebrity.Firstname,
                    updatedCelebrity.Surname,
                    updatedCelebrity.PhotoPath
                );

                celebrities[index] = newCelebrity;
                isModified = true;
                return newCelebrity.Id;
            }
            return null;
        }

        public int SaveChanges()
        {
            if (!isModified) return 0;

            string jsonPath = Path.Combine(BasePath, JSONFileName);
            string json = JsonSerializer.Serialize(celebrities, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonPath, json);
            isModified = false;
            return celebrities.Count;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                // Если что-то модифицировалось — сохранить перед выходом
                SaveChanges();
                disposed = true;
            }
        }
    }

}
