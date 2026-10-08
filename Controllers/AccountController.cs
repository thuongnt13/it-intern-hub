using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ITInternHub.Models.ViewModels;
using ITInternHub.Services;

namespace ITInternHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IAuditService _auditService;

        public AccountController(IAuthService authService, IAuditService auditService)
        {
            _authService = authService;
            _auditService = auditService;
        }

        // ==========================================
        // 1. ĐĂNG KÝ
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Roles = await _authService.GetRegistrableRolesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _authService.GetRegistrableRolesAsync();
                return View(model);
            }

            var (success, message) = await _authService.RegisterAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.Roles = await _authService.GetRegistrableRolesAsync();
                return View(model);
            }

            // Đăng ký xong có tài khoản mới cho đăng nhập
            TempData["SuccessMessage"] = "Đăng ký thành công! Hãy đăng nhập bằng tài khoản vừa tạo.";
            return RedirectToAction(nameof(Login));
        }

        // ==========================================
        // 2. ĐĂNG NHẬP
        // ==========================================
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("ROLE_ADMIN"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, message, userResult) = await _authService.ValidateLoginAsync(model);
            if (!success || userResult == null)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            // Tạo danh sách Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userResult.UserId.ToString()),
                new Claim(ClaimTypes.Name, userResult.Username),
                new Claim(ClaimTypes.GivenName, userResult.FullName),
                new Claim(ClaimTypes.Email, userResult.Email)
            };

            // Đưa tất cả Roles vào Claims
            foreach (var role in userResult.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Đưa tất cả Permissions vào Claims
            foreach (var permission in userResult.Permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );
            await _auditService.LogAsync(
                            userId: userResult.UserId,
                            action: "LOGIN_SUCCESS",
                            tableName: "auth.users",
                            recordId: userResult.UserId,
                            description: $"Người dùng [{userResult.Username}] đăng nhập thành công vào hệ thống với các vai trò: {string.Join(", ", userResult.Roles)}"
                        );

            TempData["SuccessMessage"] = $"Xin chào {userResult.FullName}!";

            // NẾU LÀ ADMIN HOẶC CÁN BỘ NHÀ TRƯỜNG -> ĐẨY VÀO KHU VỰC QUẢN TRỊ ADMIN
            if (userResult.Roles.Contains("ROLE_ADMIN") || userResult.Roles.Contains("ROLE_UNIVERSITY"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            // Nếu có ReturnUrl hợp lệ
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // Các vai trò khác (Sinh viên, Doanh nghiệp) -> Về Trang chủ
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 3. ĐĂNG XUẤT
        // ==========================================
        [HttpGet, HttpPost]
        public async Task<IActionResult> Logout()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(userIdClaim, out var userId))
                {
                    await _auditService.LogAsync(
                        userId: userId,
                        action: "LOGOUT",
                        tableName: "auth.users",
                        recordId: userId,
                        description: $"Người dùng [{User.Identity.Name}] đã đăng xuất khỏi hệ thống."
                    );
                }
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 4. TRANG 403 FORBIDDEN
        // ==========================================
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}