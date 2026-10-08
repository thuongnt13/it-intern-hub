namespace ITInternHub.Services
{
    public interface IAuditService
    {
        Task LogAsync(long userId, string action, string tableName, long? recordId, string description);
    }
}