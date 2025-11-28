using ECommerce.API.Admin.Application.Errors;
using System.Net;

namespace ECommerce.API.Admin.Application.Extensions
{
    public static class PaginationExtensions
    {
        public static Pagination BuildPagination(this (int total, int page, int pageSize) info)
        {
            var (total, page, pageSize) = info;
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
            return new Pagination
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = totalPages == 0 ? 1 : totalPages
            };
        }

        public static AppPaginatedResponse<T> NotFoundPageResult<T>(this Pagination pagination)
        {
            return new AppPaginatedResponse<T>(
                Enumerable.Empty<T>(),
                pagination,
                (int)HttpStatusCode.NotFound,
        
                new List<string> { "Page not found." }
               );
        }

        public static AppPaginatedResponse<T> EmptyPageResult<T>(int pageSize)
        {
            return new AppPaginatedResponse<T>(
                Enumerable.Empty<T>(),
                new Pagination
                {
                    CurrentPage = 1,
                    PageSize = pageSize,
                    TotalItems = 0,
                    TotalPages = 0
                },
                (int)HttpStatusCode.OK,
                new List<string>
                {
                    "No items found."
                }
               );
        }
    }
}
