using Dsw2025Tpi.Application.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
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
                NoContentException => HttpStatusCode.NoContent,
                DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("duplicate") == true => HttpStatusCode.Conflict,
                DbUpdateException => HttpStatusCode.BadRequest,
                DatabaseUnavailableException => HttpStatusCode.ServiceUnavailable,
                RoleSeedingException => HttpStatusCode.InternalServerError,
                UserSeedingException => HttpStatusCode.InternalServerError,
                Application.Exceptions.ApplicationException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };

            string errorMessage = ex.Message;

            var errorsProperty = ex.GetType().GetProperty("Errors");
            if (errorsProperty != null)
            {
                if (errorsProperty.GetValue(ex) is IEnumerable<object> errors)
                {
                    // 🔥 Convertir el array de errores en string separado por ", "
                    var joinedErrors = errors
                        .Select(e =>
                        {
                            var descProp = e.GetType().GetProperty("Description");
                            var codeProp = e.GetType().GetProperty("Code");

                            // IdentityError tiene Description, otros tipos pueden tener Message
                            if (descProp != null)
                                return descProp.GetValue(e)?.ToString();

                            if (codeProp != null)
                                return codeProp.GetValue(e)?.ToString();

                            var msgProp = e.GetType().GetProperty("Message");
                            if (msgProp != null)
                                return msgProp.GetValue(e)?.ToString();

                            return e.ToString();
                        })
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();

                    if (joinedErrors.Any())
                    {
                        errorMessage = string.Join(", ", joinedErrors);
                    }
                }
            }

            var result = JsonSerializer.Serialize(new
            {
                error = errorMessage,
                inner = ex.InnerException?.Message,
                type = ex.GetType().Name,
                status = (int)status
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            return context.Response.WriteAsync(result);
        }
    }
}
