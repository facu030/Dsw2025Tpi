using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Dsw2025Tpi.Application.Exceptions;



namespace Dsw2025Tpi.Api.Middleware
    {
        public class ExceptionHandlingMiddleware 
        {
            private readonly RequestDelegate _next; //Representa el Siguiente  Componente en la pipeline o Tuberia 
            private readonly ILogger<ExceptionHandlingMiddleware> _logger; //Se logea la excepcion que ocurra

            public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task Invoke(HttpContext context) 
            {
                try
                {
                    await _next(context);  //intenta Seguir con el Proximo Pipeline 
                }
                catch (Exception ex) //Si lanza una excepcion
                {
                    _logger.LogError(ex, "Se produjo una excepción no controlada"); //loguea el error
                    await HandleExceptionAsync(context, ex); //Llama a esta excepcion para construir la Respuesta Http adecuada
                }
            }

            private static Task HandleExceptionAsync(HttpContext context, Exception ex)
            {
                var code = ex switch //Determina el codigo de Estado Del Metodo HTTP
                {
                    NotFoundException => HttpStatusCode.NotFound, //404
                    InvalidOrderDataException => HttpStatusCode.BadRequest, //400
                    InsufficientStockException => HttpStatusCode.BadRequest, //400
                    DuplicatedEntityException => HttpStatusCode.Conflict, //409
                    _ => HttpStatusCode.InternalServerError //500
                };

                var result = JsonSerializer.Serialize(new //construimos un obj JSon con el msj y codigo
                {
                    error = ex.Message,
                    statusCode = (int)code   
                });
             
                context.Response.ContentType = "application/json"; //configura la Respuesta HTTP
                context.Response.StatusCode = (int)code;
                return context.Response.WriteAsync(result);
            }
        }
  }







