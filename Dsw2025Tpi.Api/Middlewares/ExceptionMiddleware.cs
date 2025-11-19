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
                DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("duplicate") == true => HttpStatusCode.Conflict,
                DbUpdateException => HttpStatusCode.BadRequest,
                DatabaseUnavailableException => HttpStatusCode.ServiceUnavailable,
                RoleSeedingException => HttpStatusCode.InternalServerError,
                UserSeedingException => HttpStatusCode.InternalServerError,
                _ => HttpStatusCode.InternalServerError
            };

            var errorMessage = ex switch
            {
                DuplicatedEntityException => ex.Message,
                EntityNotFoundException => ex.Message,
                ArgumentException => ex.Message,
                DbUpdateException => ex.Message,
                DatabaseUnavailableException => ex.Message,
                RoleSeedingException => ex.Message,
                UserSeedingException => ex.Message,
                _ => ex.Message
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
