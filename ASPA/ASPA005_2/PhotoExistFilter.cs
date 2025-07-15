using DAL004;

namespace ASPA005_2
{
    public class PhotoExistFilter : IEndpointFilter
    {
        private readonly IRepository _repository;

        public PhotoExistFilter(IRepository repository)
        {
            _repository = repository;
        }
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var celebrity = context.GetArgument<Celebrity>(0);
            if (celebrity == null)
                throw new AddCelebrityException("/Celebrities error, Celebrity object is null");

            var fileName = Path.GetFileName(celebrity.PhotoPath);
            var basePath = "C:\\Users\\user\\Desktop\\VS\\ТПвИ\\ASPA\\ASPA005_1\\bin\\Debug\\net8.0\\Celebrities";
            var fullPath = Path.Combine(basePath, fileName);

            if (!File.Exists(fullPath))
            {
                context.HttpContext.Response.Headers["X-Celebrity"] = $"Not Found = {fileName}";
            }

            return await next(context);
        }
    }
}
