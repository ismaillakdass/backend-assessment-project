namespace Assessment.Domain.Exceptions;

public class BusinessException : Exception
{
    public string? ErrorCode { get; }

    public BusinessException(string message, string? errorCode = null) : base(message)
    {
        ErrorCode = errorCode;
    }
}
