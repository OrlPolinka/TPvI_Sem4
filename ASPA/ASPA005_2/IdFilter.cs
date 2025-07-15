using DAL004;

namespace ASPA005_2
{
    public class IdFilter : IEndpointFilter
    {
        private readonly IRepository _repository;

        public IdFilter(IRepository repository)
        {
            _repository = repository;
        }
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            int id = context.GetArgument<int>(0);

            if (_repository.getCelebrityById(id) == null)
            {
                return Results.Conflict($"Celebrity by id={id} Not Found");
            }

            return await next(context);
        }
    }
}
