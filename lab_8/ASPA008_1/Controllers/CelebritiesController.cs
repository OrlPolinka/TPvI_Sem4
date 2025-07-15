using Microsoft.AspNetCore.Mvc;
using DAL_Celebrity_MSSQL; // Для Celebrity и LifeEvent, IRepository
using ANC25_WEBAPI_DLL; // <- ЭТА СТРОКА ОЧЕНЬ ВАЖНА! Она делает доступным CelebritiesConfig
                        //    и его вложенные классы через CelebritiesConfig.ClassName
using ASPA008_1.Filters; // Для нашего Action-фильтра
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList
using System.Net; // Для HttpStatusCode
using System.IO; // Для работы с файлами
using Microsoft.AspNetCore.Http; // Для IFormFile
using System.Threading.Tasks; 
using Microsoft.Extensions.Logging; 
using Microsoft.Extensions.Options; // Для доступа к CelebritiesConfig
using System; 


namespace ASPA008_1.Controllers
{
    public class CelebritiesController : Controller
    {
        private readonly IRepository _repository;
        private readonly CelebritiesConfig.CountryCodes _countryCodes;
        private readonly CelebritiesConfig.CelebrityTitles _celebrityTitles; 
        private readonly ILogger<CelebritiesController> _logger;
        private readonly CelebritiesConfig _celebritiesConfig; 

        public CelebritiesController(
            IRepository repository,
            CelebritiesConfig.CountryCodes countryCodes, 
            CelebritiesConfig.CelebrityTitles celebrityTitles, 
            ILogger<CelebritiesController> logger,
            IOptions<CelebritiesConfig> celebritiesConfig)
        {
            _repository = repository;
            _countryCodes = countryCodes;
            _celebrityTitles = celebrityTitles;
            _logger = logger;
            _celebritiesConfig = celebritiesConfig.Value;
        }


        // GET: /Celebrities (Index) - отображение фотографий всех знаменитостей
        public IActionResult Index()
        {
            var celebrities = _repository.GetAllCelebrities();
            return View(celebrities);
        }

        // GET: /{id:int:min(1)} или Celebrities/Human/5 - отображение информации выбранной знаменитости
        [ServiceFilter(typeof(CelebrityWikipediaLinksAsyncFilter))] // Применяем наш Action-фильтр
        public IActionResult Human(int id)
        {
            var celebrity = _repository.GetCelebrityById(id);
            if (celebrity == null)
            {
                return NotFound();
            }
            return View("Details", celebrity); // Используем представление Details
        }

        // GET: /0 или Celebrities/NewHumanForm - добавление информации о новой знаменитости (форма)
        public IActionResult NewHumanForm()
        {
            // Создаем новую пустую модель для формы
            return View("Edit", new Celebrity());
        }

        // POST: Celebrities/Create - сохранение новой знаменитости
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF
        public async Task<IActionResult> Create([Bind("Id,FullName,Nationality,ReqPhotoPath")] Celebrity celebrity, IFormFile? photoFile)
        {
            if (ModelState.IsValid)
            {
                if (photoFile != null && photoFile.Length > 0)
                {
                    // Сохраняем файл фотографии
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), _celebritiesConfig.PhotosFolder);
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + photoFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await photoFile.CopyToAsync(fileStream);
                    }
                    celebrity.ReqPhotoPath = uniqueFileName; // Сохраняем только имя файла в БД
                }

                if (_repository.AddCelebrity(celebrity))
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Не удалось добавить знаменитость.");
            }
            // ViewBag.Countries = new SelectList(_countryCodes.Codes, "alpha_2", "name");
            return View("Edit", celebrity);
        }

        // GET: Celebrities/Edit/5 - форма редактирования существующей знаменитости
        public IActionResult Edit(int id)
        {
            var celebrity = _repository.GetCelebrityById(id);
            if (celebrity == null)
            {
                return NotFound();
            }
            // ViewBag.Countries = new SelectList(_countryCodes.Codes, "alpha_2", "name");
            return View("Edit", celebrity);
        }

        // POST: Celebrities/Edit/5 - обновление существующей знаменитости
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Nationality,ReqPhotoPath")] Celebrity celebrity, IFormFile? photoFile)
        {
            if (id != celebrity.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Обработка новой фотографии, если загружена
                    if (photoFile != null && photoFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), _celebritiesConfig.PhotosFolder);
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        // Удалить старую фотографию, если есть
                        var oldCelebrity = _repository.GetCelebrityById(id);
                        if (oldCelebrity != null && !string.IsNullOrEmpty(oldCelebrity.ReqPhotoPath))
                        {
                            var oldFilePath = Path.Combine(uploadsFolder, oldCelebrity.ReqPhotoPath);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + photoFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await photoFile.CopyToAsync(fileStream);
                        }
                        celebrity.ReqPhotoPath = uniqueFileName;
                    }
                    else // Если новая фотография не загружена, сохраняем старый путь к фото
                    {
                        var oldCelebrity = _repository.GetCelebrityById(id);
                        if (oldCelebrity != null)
                        {
                            celebrity.ReqPhotoPath = oldCelebrity.ReqPhotoPath;
                        }
                    }

                    if (_repository.UpdateCelebrity(id, celebrity))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        // Если update вернул false, возможно, знаменитость не найдена или другая проблема
                        ModelState.AddModelError("", "Не удалось обновить знаменитость. Возможно, она не существует.");
                        _logger.LogWarning($"Attempt to update non-existent celebrity with ID: {id}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating celebrity with ID: {id}");
                    ModelState.AddModelError("", "Произошла ошибка при обновлении знаменитости.");
                }
            }
            // ViewBag.Countries = new SelectList(_countryCodes.Codes, "alpha_2", "name");
            return View("Edit", celebrity);
        }


        // GET: Celebrities/Delete/5 - форма подтверждения удаления
        public IActionResult Delete(int id)
        {
            var celebrity = _repository.GetCelebrityById(id);
            if (celebrity == null)
            {
                return NotFound();
            }
            return View("Delete", celebrity);
        }

        // POST: Celebrities/DeleteConfirmed/5 - выполнение удаления
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var celebrity = _repository.GetCelebrityById(id);
            if (celebrity == null)
            {
                return NotFound(); 
            }

            // Удаляем файл фотографии, если есть
            if (!string.IsNullOrEmpty(celebrity.ReqPhotoPath))
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), _celebritiesConfig.PhotosFolder);
                var filePath = Path.Combine(uploadsFolder, celebrity.ReqPhotoPath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            if (_repository.DeleteCelebrity(id))
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Если delete вернул false, возможно, знаменитость не найдена или другая проблема
                _logger.LogWarning($"Attempt to delete non-existent celebrity with ID: {id}");
                return RedirectToAction(nameof(Index)); // В любом случае перенаправляем, если объект не найден или удален
            }
        }
    }
}