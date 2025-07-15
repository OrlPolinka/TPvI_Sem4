using DAL004;

namespace ASPA005_2
{
    public class SurnameFilter : IEndpointFilter
    {
        private readonly IRepository _repository;

        public SurnameFilter(IRepository repository)
        {
            _repository = repository;
        }
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var celebrity = context.GetArgument<Celebrity>(0);
            if (celebrity == null)
                throw new AddCelebrityException("/Celebrities error, Celebrity object is null");

            if (string.IsNullOrWhiteSpace(celebrity.Surname) || celebrity.Surname.Length < 2)
                throw new PostException("/Celebrities error, Surname is wrong");



            //if (repository.getAllCelebrities().Any(c => c.Surname == celebrity.Surname))
            var existingCelebrities = _repository.getAllCelebrities();
            if (existingCelebrities.Any(c => c.Surname.Equals(celebrity.Surname, StringComparison.OrdinalIgnoreCase)))
                throw new PostException("/Celebrities error, Surname is doubled");

            return await next(context);
        }
    }
}
