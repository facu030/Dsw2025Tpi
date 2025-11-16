using Dsw2025Tpi.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace Dsw2025Tpi.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
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

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var status = ex switch
            {
                ArgumentException => HttpStatusCode.BadRequest,
                EntityNotFoundException => HttpStatusCode.NotFound,
                DuplicatedEntityException => HttpStatusCode.Conflict,
                DbUpdateException => HttpStatusCode.Conflict,
                _ => HttpStatusCode.InternalServerError
            };

            var result = JsonSerializer.Serialize(new
            {
                error = ex.Message,
                type = ex.GetType().Name,
                status = (int)status
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            return context.Response.WriteAsync(result);
        }
    }
}
