using ITInternHub.Models;
using ITInternHub.Models.Entities;

namespace ITInternHub.Services
{
    public class AuditService : IAuditService
    {
        private readonly ITInternHubDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ITInternHubDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(long userId, string action, string tableName, long? recordId, string description)
        {
            try
            {
                // Lấy địa chỉ IP của máy khách gọi vào
                var httpContext = _httpContextAccessor.HttpContext;
                var ip = httpContext?.Connection?.RemoteIpAddress?.ToString();
                if (string.IsNullOrEmpty(ip) || ip == "::1")
                {
                    ip = "127.0.0.1 (Localhost)";
                }

                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    TableName = tableName,
                    RecordId = recordId,
                    Description = description,
                    IpAddress = ip,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Cơ chế an toàn (Fail-safe): Không làm gián đoạn luồng chính nếu việc ghi log gặp trục trặc
            }
        }
    }
}