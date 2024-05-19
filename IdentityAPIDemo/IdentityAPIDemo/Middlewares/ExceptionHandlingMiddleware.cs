using IdentityAPIDemo.Models;
using System.Net;

namespace IdentityAPIDemo.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var errorResponse = new ErrorResponse() { TimeStamp = DateTime.UtcNow, Error = exception.Message };
            switch (exception)
            {
                //có thể thêm vào nhiều các xử lý đối với từng loại status code mà mình nhận được
                case BadHttpRequestException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    _logger.LogInformation(exception.Message);
                    break;
                default:
                    //đối với những status code chưa biết -> cứ trả về InternalServerError
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    //thực hiện log ra exception đang bị
                    _logger.LogError(exception.ToString());
                    break;
            }
            var result = errorResponse.ToString();
            await context.Response.WriteAsync(result);
        }
    }
}
