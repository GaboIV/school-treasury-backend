using API.Middleware;
using Microsoft.AspNetCore.Builder;

namespace API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleAuthorizationMiddleware>();
        }
    }
} 