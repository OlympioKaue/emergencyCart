using EmergencyCart.Application.SharedContext.Results;

namespace EmergencyCart.API.Middlewares;


public sealed class ExceptionHandlingMiddleware(RequestDelegate next)
{

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            await HandleValidationExceptionAsync(context, exception);
        }
        catch (Exception)
        {
            await HandleUnknownExceptionAsync(context);
        }
    }

    private static async Task HandleValidationExceptionAsync(
        HttpContext context,
        ValidationException exception)
    {
        var errors = exception.Errors
            .SelectMany(error => error.Messages)
            .ToArray();

        var response = new ErrorResponse(errors);

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(response);
    }

    private static async Task HandleUnknownExceptionAsync(
        HttpContext context)
    {
        var response = new ErrorResponse(
            ["Ocorreu um erro interno no servidor."]);

        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(response);
    }
}

