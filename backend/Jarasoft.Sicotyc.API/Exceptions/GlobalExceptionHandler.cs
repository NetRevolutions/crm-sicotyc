using Jarasoft.Sicotyc.Application.Common.Exceptions;
using Jarasoft.Sicotyc.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Jarasoft.Sicotyc.API.Exceptions;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException =>
                (
                    StatusCodes.Status400BadRequest,
                    "Validation error"
                ),

            UnauthorizedException =>
                (
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized"
                ),

            ForbiddenException =>
                (
                    StatusCodes.Status403Forbidden,
                    "Forbidden"
                ),

            NotFoundException =>
                (
                    StatusCodes.Status404NotFound,
                    "Not Found"
                ),

            ConflictException =>
                (
                    StatusCodes.Status409Conflict,
                    "Conflict"
                ),

            _ =>
                (
                    StatusCodes.Status500InternalServerError,
                    "Server error"
                )
        };

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = statusCode switch
                    {
                        StatusCodes.Status400BadRequest =>
                            "Bad Request",

                        StatusCodes.Status409Conflict =>
                            "Conflict",

                        _ =>
                            "Internal Server Error"
                    },

                    Detail =
                        statusCode ==
                        StatusCodes.Status500InternalServerError
                            ? "Ocurrió un error interno en el servidor."
                            : exception.Message
                }
            });
    }
}