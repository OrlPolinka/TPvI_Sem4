using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DAL_Celebrity_MSSQL; // Для Celebrity
using System.Collections.Generic;
using System.Text.Json;

namespace ASPA008_1.Filters
{
    


    public class CelebrityWikipediaLinksAsyncFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Выполнить действие контроллера
            var executedContext = await next();

            // Получить результат и модель
            if (executedContext.Result is ViewResult viewResult &&
                viewResult.Model is Celebrity celebrity)
            {
                var wikiLinksDict = await WikiInfoCelebrity.GetReferences(celebrity.FullName);

                // Передаем словарь в ViewData (ключ — название статьи, значение — ссылка)
                viewResult.ViewData["WikipediaLinksDict"] = wikiLinksDict;
            }
        }
    }

    public class WikiInfoCelebrity//для получения информации из Wikipedia по имени знаменитости.
    {
        private readonly HttpClient _client;
        private readonly Dictionary<string, string> _wikiReferences;
        private readonly string _wikiURI;

        private WikiInfoCelebrity(string fullName)
        {
            _client = new HttpClient();
            _wikiReferences = new Dictionary<string, string>();
            _wikiURI = $"https://en.wikipedia.org/w/api.php?action=opensearch&search={Uri.EscapeDataString(fullName)}&prop=info&format=json";
        }


        public static async Task<Dictionary<string, string>> GetReferences(string fullName)
        {
            var info = new WikiInfoCelebrity(fullName);
            HttpResponseMessage message = await info._client.GetAsync(info._wikiURI);//оздаём объект и выполняем GET-запрос к Wikipedia API.
            /* Если запрос успешен — читаем JSON-ответ как список объектов (в структуре Wikipedia OpenSearch).*/
            if (message.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await message.Content.ReadFromJsonAsync<List<object>>();

                if (result != null && result.Count >= 4)
                {
                    var titles = JsonSerializer.Deserialize<List<string>>(result[1].ToString());// Извлекаем заголовки (result[1]) и ссылки (result[3]) из массива результата.
                    var urls = JsonSerializer.Deserialize<List<string>>(result[3].ToString());// Сериализуем каждый элемент в List<string>.

                    if (titles != null && urls != null && titles.Count == urls.Count)
                    {
                        for (int i = 0; i < titles.Count; i++)
                        {
                            info._wikiReferences[titles[i]] = urls[i];
                        }
                    }
                }
            }

            return info._wikiReferences;
        }
    }

}