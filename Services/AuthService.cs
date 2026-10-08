using Microsoft.EntityFrameworkCore;
using ITInternHub.Models;
using ITInternHub.Models.Entities;
using ITInternHub.Models.ViewModels;

namespace ITInternHub.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITInternHubDbContext _context;

        public AuthService(ITInternHubDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách Role cho phép tự do đăng ký (Không bao gồm ROLE_ADMIN)
        public async Task<List<Role>> GetRegistrableRolesAsync()
        {
            return await _context.Roles
                .Where(r => r.IsActive && r.IsRegistrable)
                .OrderBy(r => r.Id)
                .ToListAsync();
        }

        // F1.1: Đăng ký tài khoản mới và gán Role vào auth.user_roles
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
        {
            // 1. Kiểm tra tài khoản hoặc email đã tồn tại chưa
            var existingUser = await _context.Users
                .AnyAsync(u => u.Username == model.Username || u.Email == model.Email);
            if (existingUser)
            {
                return (false, "Tên đăng nhập hoặc Email này đã được sử dụng.");
            }

            // 2. Kiểm tra Role hợp lệ và có quyền tự đăng ký không
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == model.SelectedRoleId && r.IsRegistrable && r.IsActive);
            if (role == null)
            {
                return (false, "Vai trò được chọn không hợp lệ.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 3. Tạo User cơ bản
                var newUser = new User
                {
                    Username = model.Username.Trim(),
                    FullName = model.FullName.Trim(),
                    Email = model.Email.Trim().ToLower(),
                    Phone = model.Phone?.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // 4. Gán quyền vào bảng trung gian auth.user_roles (N:N)
                var userRole = new UserRole
                {
                    UserId = newUser.Id,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow
                };
                _context.UserRoles.Add(userRole);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Đăng ký thành công!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        // F1.3: Đăng nhập, kiểm tra mật khẩu & trích xuất gộp tất cả Roles và Permissions (DISTINCT)
        public async Task<(bool Success, string Message, AuthUserResult? UserResult)> ValidateLoginAsync(LoginViewModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail) && u.DeletedAt == null);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return (false, "Tên đăng nhập hoặc mật khẩu không chính xác.", null);
            }

            if (!user.IsActive)
            {
                return (false, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.", null);
            }

            // 1. Trích xuất tất cả Roles của User
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Code)
                .Distinct()
                .ToListAsync();

            // 2. Trích xuất tất cả Permissions (DISTINCT) từ tất cả Roles mà user nắm giữ
            var permissions = await (from ur in _context.UserRoles
                                     join rp in _context.RolePermissions on ur.RoleId equals rp.RoleId
                                     join p in _context.Permissions on rp.PermissionId equals p.Id
                                     where ur.UserId == user.Id
                                     select p.Code)
                                     .Distinct()
                                     .ToListAsync();

            // Cập nhật ngày đăng nhập cuối
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var result = new AuthUserResult
            {
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Roles = roles,
                Permissions = permissions
            };

            return (true, "Đăng nhập thành công.", result);
        }
    }
}