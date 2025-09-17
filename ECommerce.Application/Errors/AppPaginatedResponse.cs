namespace ECommerce.API.Admin.Application.Errors
{
    public class AppPaginatedResponse<T> : AppResponse<IEnumerable<T>>
    {
        public Pagination Pagination { get; set; }

        public AppPaginatedResponse(IEnumerable<T> data, Pagination pagination, int statusCode = 200, List<string> errors = null)
            : base(data, statusCode, errors)
        {
            Pagination = pagination;
        }
    }
}
