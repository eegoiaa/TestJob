namespace TestJob.Api.Models;

public class ProcessingException : Exception
{
    public string ErrorCode { get; }

    public ProcessingException(string errorCode, string message)
        : base(message) => ErrorCode = errorCode;

    public ProcessingException(string errorCode, string message, Exception inner)
        : base(message, inner) => ErrorCode = errorCode;
}
