using System.Net;

namespace ECommerce.API.Admin.Application.Errors
{
    public class AppResponse<T>
    {
        public bool Success { get; set; } = true;
        public T Data { get; set; }
        public int StatusCode { get; set; } = 200;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Holds both success and error messages.
        /// </summary>
        public List<string> Errors { get; set; } = new();

        public AppResponse(T data, int statusCode, List<string> errors = null)
        {
            Data = data;
            StatusCode = IsValid(statusCode) ? statusCode : 500;

            if (errors != null && errors.Any())
            {
                Errors = errors;
                Success = false;
            }
            else
            {
                Success = statusCode >= 200 && statusCode < 300;
            }
        }

        public static AppResponse<T> SuccessResult(T data, int statusCode = 200) =>
            new AppResponse<T>(data, statusCode);

        public static AppResponse<T> ErrorResult(List<string> errors, int statusCode = 400) =>
            new AppResponse<T>(default, statusCode, errors);

        private static bool IsValid(int statusCode)
        {
            return Enum.IsDefined(typeof(HttpStatusCode), statusCode);
        }
    }
}
