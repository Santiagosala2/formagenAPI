using FormagenAPI.Exceptions;
using System.Net;

namespace FormagenAPI.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;


        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<ErrorHandlingMiddleware> logger)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse();

                if (ex is FormNotFoundException)
                {
                    errorResponse.Message = "Form is not found";
                    context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                }

                if (ex is CreateFormException)
                {
                    errorResponse.Message = "Create form failed";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

                if (ex is UpdateFormException)
                {
                    errorResponse.Message = "Update form failed";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

                if (ex is DeleteFormException)
                {
                    errorResponse.Message = "Delete form failed";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

                if (ex is Exception)
                {
                    errorResponse.Message = "Something went wrong";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

                logger.LogError(ex.Message, ex?.InnerException);
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }

    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
