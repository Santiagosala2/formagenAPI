using FormagenAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class UserAuthorizeSessionAttribute : Attribute, IAsyncAuthorizationFilter
{

    public UserAuthorizeSessionAttribute()
    {
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var sessionId = httpContext.Request.Cookies["UserSessionId"];

        if (string.IsNullOrEmpty(sessionId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userService = httpContext.RequestServices.GetService<IUserService>();
        var session = await userService!.GetSessionByIdAsync(sessionId);
        session.IsAdmin = false;
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
