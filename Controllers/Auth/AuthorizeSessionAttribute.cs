using FormagenAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Models;

public class AuthorizeSessionAttribute : Attribute, IAsyncAuthorizationFilter
{

    public AuthorizeSessionAttribute()
    {
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var adminSessionId = httpContext.Request.Cookies["SessionId"];
        var userSessionId = httpContext.Request.Cookies["UserSessionId"];

        if (string.IsNullOrEmpty(adminSessionId) && string.IsNullOrEmpty(userSessionId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var session = new Session();
        if (!string.IsNullOrEmpty(userSessionId))
        {
            var userService = httpContext.RequestServices.GetService<IUserService>();
            session = await userService!.GetSessionByIdAsync(userSessionId);
            session.IsAdmin = false;
        }

        if (!string.IsNullOrEmpty(adminSessionId))
        {
            var adminService = httpContext.RequestServices.GetService<IAdminService>();
            session = await adminService!.GetSessionByIdAsync(adminSessionId);
            session.IsAdmin = true;
        }

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
