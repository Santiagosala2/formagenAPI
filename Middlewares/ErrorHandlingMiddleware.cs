using FormagenAPI.Exceptions;
using Microsoft.Azure.Cosmos;
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
                    errorResponse.Message = ex.Message;
                    errorResponse.StatusCode = HttpStatusCode.NotFound;
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                if (ex is FormNameIsNotUniqueException)
                {
                    errorResponse.Message = ex.Message;
                    errorResponse.StatusCode = HttpStatusCode.BadRequest;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }


                if (ex is UserEmailNotUniqueException)
                {
                    errorResponse.Message = ex.Message;
                    errorResponse.StatusCode = HttpStatusCode.BadRequest;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }

                if (ex is AdminSessionNotFoundException)
                {
                    errorResponse.Message = ex.Message;
                    errorResponse.StatusCode = HttpStatusCode.BadRequest;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }




                if (ex is UnexpectedCosmosException)
                {
                    errorResponse.Message = "Something went wrong";
                    errorResponse.StatusCode = HttpStatusCode.InternalServerError;
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
