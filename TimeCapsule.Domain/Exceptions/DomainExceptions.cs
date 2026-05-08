namespace TimeCapsule.Domain.Exceptions;

public class AppException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    public AppException(string errorCode, string message, int statusCode = 400) : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string errorCode, string message) : base(errorCode, message, 401) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string errorCode, string message) : base(errorCode, message, 404) { }
}

public class ConflictException : AppException
{
    public ConflictException(string errorCode, string message) : base(errorCode, message, 409) { }
}
