using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASPA007_1.Pages
{
    public class NewCelebrityModel : PageModel
    {
        private readonly IRepository _repository;
        private readonly IWebHostEnvironment _env;
        public NewCelebrityModel(IRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }

        [BindProperty]
        public string FullName { get; set; }

        [BindProperty]
        public string Nationality { get; set; }

        [BindProperty]
        public IFormFile Photo { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid || Photo == null)
                return Page();

            string photosPath = Path.Combine(_env.WebRootPath, "Photos");
            string fileName = Path.GetFileName(Photo.FileName);
            string filePath = Path.Combine(photosPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                Photo.CopyTo(stream);
            }

            var celeb = new Celebrity
            {
                FullName = FullName,
                Nationality = Nationality,
                ReqPhotoPath = fileName
            };

            _repository.AddCelebrity(celeb);

            return RedirectToPage("/Celebrities");
        }
    }
}
