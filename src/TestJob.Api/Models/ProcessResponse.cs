namespace TestJob.Api.Models;

public class ProcessResponse
{
    public int IsError { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public int ElementsCount { get; set; }
    public int EmailsCount { get; set; }
    public string Url { get; set; } = string.Empty;
    public string DecryptedPlainText { get; set; } = string.Empty;
    public List<string> ElementsAttrList { get; set; } = [];
    public List<string> EmailsList { get; set; } = [];

    public static ProcessResponse Error(string code, string message) => new()
    {
        IsError = 1,
        ErrorCode = code,
        ErrorMessage = message
    };
}
