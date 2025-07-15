using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASPA007_1.Pages
{
    public class CelebrityModel : PageModel
    {
        private readonly IRepository _repository;

        public CelebrityModel(IRepository repository)
        {
            _repository = repository;
        }

        public Celebrity? Celeb { get; private set; }

        public List<LifeEvent>? LifeEvents { get; private set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public IActionResult OnGet()
        {
            Celeb = _repository.GetCelebrityById(Id);
            if (Celeb == null)
                return NotFound();

            LifeEvents = _repository.GetLifeEventsByCelebrityId(Id);

            return Page();
        }
    }

}
