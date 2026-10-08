namespace ITInternHub.Core.Domain.Exceptions
{
    // Ngoại lệ ném ra khi vi phạm quy tắc nghiệp vụ miền
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}