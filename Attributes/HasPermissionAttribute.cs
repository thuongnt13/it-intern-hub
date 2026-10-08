using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ITInternHub.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class HasPermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public HasPermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // 1. Chưa đăng nhập -> Yêu cầu đăng nhập
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new ChallengeResult();
                return;
            }

            // 2. Nếu là ROLE_ADMIN -> Toàn quyền truy cập (Bypass)
            if (user.IsInRole("ROLE_ADMIN"))
            {
                return;
            }

            // 3. Kiểm tra User có mã Claim Permission tương ứng hay không
            var hasPermission = user.Claims.Any(c => c.Type == "Permission" && c.Value == _permission);
            if (!hasPermission)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", new { area = "" });
            }
        }
    }
}