using FormagenAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AuthorizeSessionAttribute : Attribute, IAsyncAuthorizationFilter
{

    public AuthorizeSessionAttribute()
    {
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var sessionId = httpContext.Request.Cookies["SessionId"];

        if (string.IsNullOrEmpty(sessionId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var adminService = httpContext.RequestServices.GetService<IAdminService>();
        var session = await adminService!.GetSessionByIdAsync(sessionId);

        if (session is not null)
        {
            if (session.ExpiresAt < DateTime.UtcNow)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

        }
        httpContext.Items["Session"] = session;
    }
}
