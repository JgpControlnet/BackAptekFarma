namespace AptekFarma.Services
{
    public class MiddlewareService
    {
        public class RestrictDocumentsMiddleware
        {
            private readonly RequestDelegate _next;

            public RestrictDocumentsMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                if (context.Request.Path.StartsWithSegments("/campannas/pdf"))
                {
                        Console.WriteLine("Acceso denegado: usuario no autenticado.");
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return;
                }

                await _next(context);
            }
        }
    }
}
